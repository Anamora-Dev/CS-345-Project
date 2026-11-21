using UnityEngine;

[CreateAssetMenu(menuName = "Food/Recipe")]
public class Recipe : ScriptableObject
{

    [Header("Ticket")]
    public Sprite ticketIcon;
    [Tooltip("One line each on the ticket, e.g. Black / Oat milk")]
    public string[] ticketSpecs;
    [Tooltip("Expected cook/prep time in seconds for this item on a ticket.")]
    public float expectedCookTime = 15f;
    
    public enum VisualAction
    {
        AddLayer,
        ReplaceWholePlate,
        ReplacePreviousLayer
    }

    public enum StepPlacementType
    {
        DefaultAnchor,
        CustomSnapPoint,
        ReplaceWholeSprite // Legacy value
    }

    public enum OutputMode
    {
        SpawnOnce,       // cooked item pops out once and the vessel resets (pan, toaster)
        BecomeSpawner    // vessel turns into a spawner of the cooked item (soup pot: scoop servings out)
    }

    [System.Serializable]
    public struct Step
    {
        [Header("Ingredient")]
        public IngredientData ingredient;

        [Header("Visual Behavior")]
        public VisualAction visualAction;
        public StepPlacementType placementType;

        [Tooltip("Which snap point to use when CustomSnapPoint is selected.")]
        public int snapPointIndex;

        [Tooltip("Which earlier recipe step's visual to replace. Used by ReplacePreviousLayer.")]
        public int targetStepIndex;

        [Header("Visual")]
        public Sprite plateSprite;
        public GameObject plateVisual;

        [Header("Sorting (optional)")]
        [Tooltip("Put this step's visual on a different sorting layer than its prefab uses.")]
        public bool overrideSortingLayer;
        [Tooltip("Exact name from Project Settings > Tags and Layers > Sorting Layers.")]
        public string sortingLayerName;
    }

    [Header("Recipe Steps")]
    public Step[] steps;

    [Header("Cooking")]
    public float cookTime = 3f;
    public Ingredient cookedOutput;

    [Header("Output (vessels only)")]
    public OutputMode outputMode;
    [Tooltip("Optional: the vessel's sprite once cooking is done (e.g. a pot full of soup).")]
    public Sprite finishedVesselSprite;
    [Tooltip("BecomeSpawner: how many scoops before the vessel is empty again. 0 = unlimited.")]
    public int servings = 4;
}
