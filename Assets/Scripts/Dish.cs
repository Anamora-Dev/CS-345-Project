using UnityEngine;
using UnityEngine.Serialization;

public abstract class Dish : MonoBehaviour, IDropTarget
{
    [Header("Renderers")]
    [FormerlySerializedAs("plateRenderer")]
    [FormerlySerializedAs("bowlRenderer")]
    [SerializeField] SpriteRenderer baseRenderer;

    [SerializeField] SpriteRenderer foodRenderer;

    [Header("Visual Anchors")]
    [SerializeField] Transform foodAnchor;

    [Header("Recipes")]
    [SerializeField] Recipe[] knownRecipes;

    [Header("Snap Points")]
    [SerializeField] Transform[] customSnapPoints;

    [Header("Layering")]
    [SerializeField] int orderPerStep = 10;   // gap between steps, so multi-sprite prefabs don't collide

    RecipeTracker tracker;
    StepVisualizer visuals;

    bool completed;

    public Recipe CompletedRecipe { get; private set; }

    void Awake()
    {
        tracker = new RecipeTracker(knownRecipes);
        visuals = new StepVisualizer(this, baseRenderer, foodRenderer, foodAnchor,
            customSnapPoints, orderPerStep, false, false);
    }

    protected void SetBaseSprite(Sprite sprite)
    {
        if (baseRenderer)
            baseRenderer.sprite = sprite;
    }

    public bool TryAccept(Ingredient item)
    {
        if (completed || item == null || item.data == null)
            return false;

        if (!tracker.TryAdd(item.data, out Recipe recipe, out int stepIndex))
            return false;

        visuals.Apply(recipe.steps[stepIndex], stepIndex);

        // The ingredient has been accepted.
        if (tracker.IsComplete(recipe))
        {
            completed = true;
            CompletedRecipe = recipe;
            OnRecipeComplete(recipe);
        }

        Destroy(item.gameObject);
        return true;
    }

    protected virtual void OnRecipeComplete(Recipe recipe) { }

    public void ClearDish()
    {
        visuals.Clear();
        tracker.Clear();

        completed = false;
        CompletedRecipe = null;
    }
}
