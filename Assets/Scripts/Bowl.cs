
using System.Collections.Generic;
using UnityEngine;

public class Bowl : MonoBehaviour, IDropTarget
{
    [Header("Sprites")]
    [SerializeField] SpriteRenderer bowlRenderer;
    [SerializeField] SpriteRenderer foodRenderer;

    [Header("Food Placement")]
    [SerializeField] Transform foodAnchor;

    [Header("Recipes")]
    [SerializeField] Recipe[] knownRecipes;

    readonly List<IngredientData> contents = new();
    GameObject currentVisual;

    public void Init(BowlDesign design)
    {
        bowlRenderer.sprite = design.sprite;
    }

    public bool TryAccept(IngredientData ingredient)
    {
        foreach (var recipe in knownRecipes)
        {
            int i = contents.Count;

            if (i >= recipe.steps.Length) continue;
            if (!Matches(recipe, i, ingredient)) continue;

            contents.Add(ingredient);

            ShowFood(recipe.steps[i]);

            if (contents.Count == recipe.steps.Length)
                OnRecipeComplete(recipe);

            return true;
        }

        return false;
    }

    void ShowFood(Recipe.Step step)
    {
        if (currentVisual)
            Destroy(currentVisual);

        if (step.plateVisual)
        {
            currentVisual = Instantiate(
                step.plateVisual,
                foodAnchor
            );

            currentVisual.transform.localPosition = Vector3.zero;
            foodRenderer.sprite = null;
        }
        else
        {
            foodRenderer.sprite = step.plateSprite;
        }
    }

    bool Matches(Recipe recipe, int stepIndex, IngredientData next)
    {
        for (int k = 0; k < stepIndex; k++)
        {
            if (recipe.steps[k].ingredient != contents[k])
                return false;
        }

        return recipe.steps[stepIndex].ingredient == next;
    }

    void OnRecipeComplete(Recipe recipe)
    {
        // Serve, score, etc.
    }
}
