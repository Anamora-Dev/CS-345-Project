using System.Collections.Generic;
using UnityEngine;

public interface IDropTarget
{
    bool TryAccept(IngredientData ingredient);
}

public class Plate : MonoBehaviour, IDropTarget
{
    [SerializeField] SpriteRenderer plateRenderer;
    [SerializeField] SpriteRenderer foodRenderer;
    [SerializeField] Transform foodAnchor;   // empty child where food prefabs appear
    [SerializeField] Recipe[] knownRecipes;

    readonly List<IngredientData> contents = new();
    GameObject currentVisual;

    public void Init(PlateDesign design) => plateRenderer.sprite = design.sprite;

    public bool TryAccept(IngredientData ingredient)
    {
        foreach (var recipe in knownRecipes)
        {
            int i = contents.Count;
            if (i >= recipe.steps.Length) continue;
            if (!Matches(recipe, i, ingredient)) continue;

            contents.Add(ingredient);
            ShowFood(recipe.steps[i]);
            if (contents.Count == recipe.steps.Length) OnRecipeComplete(recipe);
            return true;
        }
        return false;
    }

    void ShowFood(Recipe.Step step)
    {
        if (currentVisual) Destroy(currentVisual);

        if (step.plateVisual)
        {
            currentVisual = Instantiate(step.plateVisual, foodAnchor);
            currentVisual.transform.localPosition = Vector3.zero;
            foodRenderer.sprite = null;
        }
        else
        {
            foodRenderer.sprite = step.plateSprite;
        }
    }

    bool Matches(Recipe r, int stepIndex, IngredientData next)
    {
        for (int k = 0; k < stepIndex; k++)
            if (r.steps[k].ingredient != contents[k]) return false;
        return r.steps[stepIndex].ingredient == next;
    }

    void OnRecipeComplete(Recipe recipe) { /* serve, score, etc. */ }
}