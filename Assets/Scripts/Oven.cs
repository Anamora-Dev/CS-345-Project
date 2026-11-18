
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : MonoBehaviour
{
    public static readonly List<Oven> All = new();

    [Header("References")]
    [SerializeField] Door door;
    [SerializeField] Collider2D ovenDropZone;
    [SerializeField] Transform ovenSnapPoint;
    [SerializeField] Collider2D topDropZone;
    [SerializeField] Transform traySnapPoint;

    [Header("Sprites")]
    [SerializeField] SpriteRenderer ovenRenderer;
    [SerializeField] Sprite idleClosedSprite;
    [SerializeField] Sprite cookingSprite;

    [Header("Recipes")]
    [SerializeField] Recipe[] recipes;

    [Header("Settings")]
    [SerializeField] float leaveDistance = 0.5f;

    [Header("Feedback")]
    [SerializeField] GameObject progressBar;
    [SerializeField] Transform fillPivot;

    Tray tray;

    bool trayHidden;
    bool cooking;
    bool cooked;
    bool lastDoorOpenState = true;

    Coroutine cookRoutine;

    Vector3 fillScale;
    float fillWidth;

    bool CanLoad => door == null || door.IsOpen;

    void Awake()
    {
        if (!ovenDropZone)
            ovenDropZone = GetComponent<Collider2D>();

        if (progressBar)
            progressBar.SetActive(false);

        if (fillPivot)
        {
            fillScale = fillPivot.localScale;
            fillWidth = fillScale.x;
        }

        if (ovenRenderer && idleClosedSprite)
            ovenRenderer.sprite = idleClosedSprite;

        if (door)
            lastDoorOpenState = door.IsOpen;
    }

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    public bool Overlaps(Collider2D other)
    {
        if (other == null)
            return false;

        bool hitOven =
            ovenDropZone != null &&
            ovenDropZone.Distance(other).isOverlapped;

        bool hitTop =
            topDropZone != null &&
            topDropZone.Distance(other).isOverlapped;

        return hitOven || hitTop;
    }

    public bool TryAccept(Tray t)
    {
        if (t == null)
            return false;

        Collider2D trayCol = t.GetComponent<Collider2D>();

        if (trayCol == null)
            return false;

        // Tray dropped inside oven
        if (ovenDropZone != null &&
            ovenDropZone.Distance(trayCol).isOverlapped)
        {
            if (!CanLoad || cooking || !ovenSnapPoint)
            {
                Debug.LogWarning(
                    "[Oven] Tray rejected. CanLoad: " +
                    CanLoad + ", Cooking: " + cooking
                );

                return false;
            }

            if (tray != null && tray != t)
                return false;

            tray = t;
            SnapTrayTo(t, ovenSnapPoint);

            cooked = false;
            trayHidden = false;

            Debug.Log("[Oven] Tray accepted inside the oven!");

            return true;
        }

        // Tray dropped on top of oven
        if (topDropZone != null &&
            topDropZone.Distance(trayCol).isOverlapped)
        {
            if (!traySnapPoint)
                return false;

            if (tray == t)
                tray = null;

            SnapTrayTo(t, traySnapPoint);

            Debug.Log("[Oven] Tray placed on top of the oven!");

            return true;
        }

        return false;
    }

    void SnapTrayTo(Tray t, Transform target)
    {
        Vector3 delta = target.position - t.Center;
        delta.z = 0f;

        t.transform.position += delta;
    }

    void Update()
    {
        // Monitor door state changes
        if (door != null)
        {
            bool currentOpen = door.IsOpen;

            if (currentOpen != lastDoorOpenState)
            {
                lastDoorOpenState = currentOpen;
                HandleDoorChanged(currentOpen);
            }
        }

        if (tray == null || cooking || cooked)
            return;

        if (!trayHidden && ovenSnapPoint &&
            Vector2.Distance(tray.Center, ovenSnapPoint.position) > leaveDistance)
        {
            Debug.Log("[Oven] Tray moved away. Unassigning tray.");

            tray = null;
        }
    }

    void HandleDoorChanged(bool open)
    {
        Debug.Log(
            $"[Oven] Door changed. Open: {open}, Tray assigned: {tray != null}"
        );

        if (tray == null)
            return;

        if (open)
        {
            if (cooking)
                CancelCook();

            tray.gameObject.SetActive(true);
            trayHidden = false;
        }
        else
        {
            tray.gameObject.SetActive(false);
            trayHidden = true;

            if (!cooked)
                TryStartCooking();
        }
    }

    void TryStartCooking()
    {
        if (tray == null || cooking || cooked)
            return;

        float cookTime = GetLongestCookTime(tray);

        if (cookTime < 0f)
        {
            Debug.LogWarning(
                "[Oven] No matching ingredient recipes found on tray."
            );

            return;
        }

        cookRoutine = StartCoroutine(Cook(cookTime));
    }

    // Returns the matching one-step recipe for an ingredient.
    Recipe FindRecipeForIngredient(IngredientData data)
    {
        if (data == null || recipes == null)
            return null;

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null ||
                recipe.steps == null ||
                recipe.steps.Length != 1)
                continue;

            if (recipe.steps[0].ingredient == data)
                return recipe;
        }

        return null;
    }

    // Find the longest cook time among all recognized tray ingredients.
    float GetLongestCookTime(Tray t)
    {
        if (t == null || t.Items == null)
            return -1f;

        float longestTime = -1f;

        foreach (Ingredient item in t.Items)
        {
            if (item == null || item.data == null)
                continue;

            Recipe recipe = FindRecipeForIngredient(item.data);

            if (recipe == null)
                continue;

            longestTime = Mathf.Max(longestTime, recipe.cookTime);
        }

        return longestTime;
    }

    IEnumerator Cook(float cookTime)
    {
        cooking = true;

        Debug.Log(
            $"[Oven] Started baking for {cookTime} seconds."
        );

        if (ovenRenderer && cookingSprite)
            ovenRenderer.sprite = cookingSprite;

        if (progressBar)
            progressBar.SetActive(true);

        tray.SetLocked(true);

        float elapsed = 0f;

        while (elapsed < cookTime)
        {
            elapsed += Time.deltaTime;

            SetProgress(
                cookTime > 0f ? elapsed / cookTime : 1f
            );

            yield return null;
        }

        SetProgress(1f);

        // Replace each recognized ingredient with its cooked output.
        for (int i = 0; i < tray.Items.Length; i++)
        {
            Ingredient item = tray.Items[i];

            if (item == null || item.data == null)
                continue;

            Recipe recipe = FindRecipeForIngredient(item.data);

            if (recipe == null)
                continue;

            if (recipe.cookedOutput == null)
            {
                Debug.LogWarning(
                    $"[Oven] Recipe {recipe.name} has no cooked output."
                );

                continue;
            }

            tray.ReplaceSlot(i, recipe.cookedOutput);
        }

        tray.SetLocked(false);

        cooked = true;
        cooking = false;
        cookRoutine = null;

        if (progressBar)
            progressBar.SetActive(false);

        if (ovenRenderer && idleClosedSprite)
            ovenRenderer.sprite = idleClosedSprite;

        Debug.Log("[Oven] Baking finished successfully!");
    }

    void CancelCook()
    {
        Debug.Log("[Oven] Cooking cancelled.");

        if (!cooking)
            return;

        if (cookRoutine != null)
            StopCoroutine(cookRoutine);

        cookRoutine = null;
        cooking = false;

        if (tray != null)
            tray.SetLocked(false);

        if (progressBar)
            progressBar.SetActive(false);

        if (ovenRenderer && idleClosedSprite)
            ovenRenderer.sprite = idleClosedSprite;

        SetProgress(0f);
    }

    void SetProgress(float p)
    {
        if (!fillPivot)
            return;

        p = Mathf.Clamp01(p);

        float width = fillWidth * p;

        fillPivot.localScale = new Vector3(
            width,
            fillScale.y,
            fillScale.z
        );

        fillPivot.localPosition = new Vector3(
            -(fillWidth - width) / 2f,
            fillPivot.localPosition.y,
            fillPivot.localPosition.z
        );
    }
}
