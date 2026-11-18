using System.Collections.Generic;
using UnityEngine;

/// Draws recipe steps onto a Dish or a Vessel, so both read the same step settings the same way:
///  - AddLayer: draws the step's sprite/prefab on top.
///  - ReplaceWholePlate: clears the layers. On a Vessel (replaceSwapsBaseSprite) it also swaps the vessel's sprite.
///  - ReplacePreviousLayer: swaps out an earlier step's layer.
public class StepVisualizer
{
    readonly Component owner;
    readonly SpriteRenderer baseRenderer;
    readonly SpriteRenderer foodRenderer;
    readonly Transform foodAnchor;
    readonly Transform[] customSnapPoints;
    readonly int orderPerStep;
    readonly bool replaceSwapsBaseSprite;
    readonly bool preservePrefabWorldScale;
    readonly Dictionary<int, GameObject> active = new();

    public StepVisualizer(Component owner, SpriteRenderer baseRenderer, SpriteRenderer foodRenderer,
        Transform foodAnchor, Transform[] customSnapPoints, int orderPerStep,
        bool replaceSwapsBaseSprite, bool preservePrefabWorldScale)
    {
        this.owner = owner;
        this.baseRenderer = baseRenderer;
        this.foodRenderer = foodRenderer;
        this.foodAnchor = foodAnchor;
        this.customSnapPoints = customSnapPoints;
        this.orderPerStep = orderPerStep;
        this.replaceSwapsBaseSprite = replaceSwapsBaseSprite;
        this.preservePrefabWorldScale = preservePrefabWorldScale;
    }

    public void Apply(Recipe.Step step, int stepIndex)
    {
        // Support the old ReplaceWholeSprite setting.
        Recipe.VisualAction action = step.visualAction;
        if (step.placementType == Recipe.StepPlacementType.ReplaceWholeSprite)
            action = Recipe.VisualAction.ReplaceWholePlate;

        switch (action)
        {
            case Recipe.VisualAction.ReplaceWholePlate:
                Clear();
                if (replaceSwapsBaseSprite && step.plateSprite && baseRenderer)
                    baseRenderer.sprite = step.plateSprite;   // vessel looks different
                else
                    CreateVisual(step, stepIndex, DefaultAnchor(), stepIndex);
                break;

            case Recipe.VisualAction.AddLayer:
                CreateVisual(step, stepIndex, StepAnchor(step), stepIndex);
                break;

            case Recipe.VisualAction.ReplacePreviousLayer:
                ReplaceLayer(step, stepIndex);
                break;
        }
    }

    /// Removes every layer this visualizer added. Does not touch the base sprite.
    public void Clear()
    {
        foreach (GameObject visual in active.Values)
            if (visual) Object.Destroy(visual);

        active.Clear();

        if (foodRenderer) foodRenderer.sprite = null;
    }

    // orderIndex decides how high the visual draws: later steps draw above earlier ones.
    void CreateVisual(Recipe.Step step, int stepIndex, Transform parent, int orderIndex)
    {
        if (parent == null)
        {
            Debug.LogWarning("Missing a visual anchor.", owner);
            return;
        }

        GameObject visual;

        if (step.plateVisual)
        {
            visual = Object.Instantiate(step.plateVisual, parent);

            Vector3 prefabScale = step.plateVisual.transform.localScale;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = step.plateVisual.transform.localRotation;
            visual.transform.localScale = preservePrefabWorldScale ? Compensate(prefabScale, parent) : prefabScale;

            Freeze(visual);
            ShiftSorting(visual, orderIndex);
        }
        else if (step.plateSprite)
        {
            visual = new GameObject("Step Visual " + stepIndex);
            visual.transform.SetParent(parent, false);

            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = step.plateSprite;

            // Copy sorting settings from the existing food renderer (or sit just above the base).
            if (foodRenderer)
            {
                sr.sortingLayerID = foodRenderer.sortingLayerID;
                sr.sortingOrder = foodRenderer.sortingOrder;
                sr.color = foodRenderer.color;
                sr.sharedMaterial = foodRenderer.sharedMaterial;
            }
            else if (baseRenderer)
            {
                sr.sortingLayerID = baseRenderer.sortingLayerID;
                sr.sortingOrder = baseRenderer.sortingOrder + 1;
            }

            sr.sortingOrder += orderIndex * orderPerStep;
        }
        else
        {
            return;
        }

        ApplySortingLayer(visual, step);
        active[stepIndex] = visual;
    }

    void ReplaceLayer(Recipe.Step step, int stepIndex)
    {
        int targetIndex = step.targetStepIndex;

        if (!active.TryGetValue(targetIndex, out GameObject oldVisual) || oldVisual == null)
        {
            Debug.LogWarning($"Cannot replace step {targetIndex}: its visual does not exist.", owner);
            return;
        }

        Transform parent = oldVisual.transform.parent;
        Vector3 localPosition = oldVisual.transform.localPosition;
        Quaternion localRotation = oldVisual.transform.localRotation;
        Vector3 localScale = oldVisual.transform.localScale;

        Object.Destroy(oldVisual);
        active.Remove(targetIndex);

        if (parent == null) parent = DefaultAnchor();

        // The replacement sits at the depth of the layer it replaces.
        CreateVisual(step, stepIndex, parent, targetIndex);

        if (active.TryGetValue(stepIndex, out GameObject replacement))
        {
            replacement.transform.localPosition = localPosition;
            replacement.transform.localRotation = localRotation;
            replacement.transform.localScale = localScale;
        }
    }

    Transform StepAnchor(Recipe.Step step)
    {
        if (step.placementType == Recipe.StepPlacementType.CustomSnapPoint)
        {
            if (customSnapPoints == null || step.snapPointIndex < 0 || step.snapPointIndex >= customSnapPoints.Length)
            {
                Debug.LogWarning("Invalid snap point index: " + step.snapPointIndex, owner);
                return DefaultAnchor();
            }
            return customSnapPoints[step.snapPointIndex];
        }
        return DefaultAnchor();
    }

    Transform DefaultAnchor()
    {
        if (foodAnchor) return foodAnchor;
        if (foodRenderer) return foodRenderer.transform;
        return owner.transform;
    }

    // Lifts every sprite in the prefab by the step's depth, keeping the prefab's own internal layering.
    void ShiftSorting(GameObject visual, int orderIndex)
    {
        int shift = orderIndex * orderPerStep;
        foreach (SpriteRenderer sr in visual.GetComponentsInChildren<SpriteRenderer>(true))
            sr.sortingOrder += shift;
    }

    // Optional per-step sorting layer (e.g. the top toast above the bottom toast).
    static void ApplySortingLayer(GameObject visual, Recipe.Step step)
    {
        if (!step.overrideSortingLayer) return;

        int id = SortingLayer.NameToID(step.sortingLayerName);
        foreach (SpriteRenderer sr in visual.GetComponentsInChildren<SpriteRenderer>(true))
            sr.sortingLayerID = id;
    }

    // A step visual is decoration only: switch off dragging and colliders even if it's an ingredient prefab.
    static void Freeze(GameObject go)
    {
        foreach (Ingredient i in go.GetComponentsInChildren<Ingredient>(true)) i.enabled = false;
        foreach (Draggable d in go.GetComponentsInChildren<Draggable>(true)) d.enabled = false;
        foreach (Collider2D c in go.GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
    }

    // Children inherit the parent's scale; undo that so a prefab keeps its authored size.
    static Vector3 Compensate(Vector3 s, Transform parent)
    {
        Vector3 p = parent.lossyScale;
        return new Vector3(
            p.x != 0 ? s.x / p.x : s.x,
            p.y != 0 ? s.y / p.y : s.y,
            p.z != 0 ? s.z / p.z : s.z);
    }
}
