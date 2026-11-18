using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// Pot, pan, toaster, blender, soup pot... anything that cooks. Ingredients are added in recipe order.
/// Each step's Visual Action decides how it looks: Add Layer draws on top of the vessel, Replace Whole Plate
/// swaps the vessel's own sprite. When the recipe is complete it cooks (immediately, or once the lid is on),
/// then the recipe's Output Mode decides what happens:
///  - SpawnOnce: the cooked item appears and the vessel resets.
///  - BecomeSpawner: the vessel becomes a spawner of the cooked item (scoop soup out, bring it to a bowl)
///    until its servings run out.
public class Vessel : MonoBehaviour, IDropTarget
{
    [Header("Visuals")]
    [SerializeField] SpriteRenderer vesselRenderer;
    [SerializeField] Sprite emptySprite;
    [SerializeField] Transform visualAnchor;          // where Add Layer visuals go (defaults to this object)
    [SerializeField] Transform[] customSnapPoints;    // for steps set to Custom Snap Point
    [SerializeField] int orderPerStep = 10;
    [SerializeField] Transform outputSpot;            // where a SpawnOnce item appears (defaults to this object)
    [SerializeField] bool preserveVisualWorldScale = true;  // step prefabs keep their own size even if the vessel is scaled
    [SerializeField] bool debugLogs;

    [Header("Recipes")]
    [SerializeField] Recipe[] recipes;

    [Header("Lid (optional)")]
    [SerializeField] Draggable lid;
    [SerializeField] Transform lidSnapPoint;    // child of the lid
    [SerializeField] float snapDistance = 1f;

    [Header("Cooking feedback (optional)")]
    [SerializeField] GameObject progressBar;
    [SerializeField] Transform fillPivot;
    [SerializeField] GameObject steamEffect;

    RecipeTracker tracker;
    StepVisualizer visuals;
    Camera cam;
    Recipe readyRecipe;   // fully assembled, waiting for the lid / cooking
    bool cooking;
    bool serving;         // BecomeSpawner: done cooking, scoops can be taken
    Recipe servingRecipe;
    int servingsLeft;
    Vector3 fillScale;
    float fillWidth;

    bool HasLid => lid != null && lidSnapPoint != null;
    bool IsLidOn => HasLid && Vector2.Distance(lidSnapPoint.position, transform.position) <= snapDistance;

    void Awake()
    {
        cam = Camera.main;
        if (!vesselRenderer) vesselRenderer = GetComponent<SpriteRenderer>();

        tracker = new RecipeTracker(recipes);
        visuals = new StepVisualizer(this, vesselRenderer, null, visualAnchor, customSnapPoints,
            orderPerStep, true, preserveVisualWorldScale);

        if (emptySprite) vesselRenderer.sprite = emptySprite;
        if (progressBar) progressBar.SetActive(false);
        if (steamEffect) steamEffect.SetActive(false);
        if (fillPivot) { fillScale = fillPivot.localScale; fillWidth = fillScale.x; }
    }

    void OnEnable()  { if (lid) lid.Dropped += OnLidDropped; }
    void OnDisable() { if (lid) lid.Dropped -= OnLidDropped; }

    // ---------- adding ingredients ----------

    public bool TryAccept(Ingredient item)
    {
        if (cooking || serving || readyRecipe != null) { Log("busy, rejected " + item.name); return false; }
        if (IsLidOn) { Log("lid is on, rejected " + item.name); return false; }   // lid must be off to add things

        if (!tracker.TryAdd(item.data, out Recipe recipe, out int i))
        {
            Log($"no recipe step matches {(item.data ? item.data.name : "NULL data")} (recipes assigned: {(recipes == null ? 0 : recipes.Length)})");
            return false;
        }

        Log($"accepted {item.data.name} as step {i + 1} of {recipe.steps.Length} in {recipe.name}");
        visuals.Apply(recipe.steps[i], i);

        if (tracker.IsComplete(recipe))
        {
            readyRecipe = recipe;
            Log("recipe complete, cooking");
            TryStartCook();
        }
        return true;
    }

    // ---------- lid ----------

    void OnLidDropped(Draggable d)
    {
        if (!IsLidOn) return;

        // Snap the lid so its snap point lands exactly on the vessel.
        Vector3 offset = transform.position - lidSnapPoint.position;
        lid.transform.position += new Vector3(offset.x, offset.y, 0f);

        TryStartCook();
    }

    // ---------- cooking ----------

    void TryStartCook()
    {
        if (readyRecipe == null || cooking) return;
        if (HasLid && !IsLidOn) return;   // waiting for the lid
        StartCoroutine(Cook(readyRecipe));
    }

    IEnumerator Cook(Recipe recipe)
    {
        cooking = true;
        if (steamEffect) steamEffect.SetActive(true);
        if (progressBar) progressBar.SetActive(true);

        float t = 0f;
        while (t < recipe.cookTime)
        {
            t += Time.deltaTime;
            SetProgress(t / recipe.cookTime);
            yield return null;
        }
        SetProgress(1f);
        yield return new WaitForSeconds(0.2f);

        if (progressBar) progressBar.SetActive(false);
        if (steamEffect) steamEffect.SetActive(false);

        if (recipe.outputMode == Recipe.OutputMode.BecomeSpawner)
        {
            BeginServing(recipe);
        }
        else
        {
            if (recipe.cookedOutput)
            {
                Vector3 pos = outputSpot ? outputSpot.position : transform.position;
                Instantiate(recipe.cookedOutput, pos, Quaternion.identity);
            }
            else
            {
                Debug.LogError($"Recipe {recipe.name} has no Cooked Output.", this);
            }

            Clear();
        }
    }

    // ---------- serving (vessel becomes a spawner) ----------

    void BeginServing(Recipe recipe)
    {
        if (!recipe.cookedOutput)
            Debug.LogError($"Recipe {recipe.name} is set to Become Spawner but has no Cooked Output.", this);

        visuals.Clear();      // drop the cooking layers
        tracker.Clear();
        readyRecipe = null;
        cooking = false;

        // The finished food can replace the vessel's look (e.g. a pot full of soup).
        if (recipe.finishedVesselSprite) vesselRenderer.sprite = recipe.finishedVesselSprite;

        serving = true;
        servingRecipe = recipe;
        servingsLeft = recipe.servings > 0 ? recipe.servings : int.MaxValue;
        Log($"done cooking, {(recipe.servings > 0 ? recipe.servings.ToString() : "unlimited")} servings");
    }

    void Update()
    {
        if (!serving || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (HasLid && IsLidOn) return;   // lid must be off to scoop

        Vector3 w = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        bool onMe = false;
        foreach (Collider2D c in Physics2D.OverlapPointAll(w))
        {
            if (c.GetComponentInParent<Draggable>() != null) return;   // grabbing something else (e.g. the lid)
            if (c.GetComponentInParent<Vessel>() == this) onMe = true;
        }
        if (!onMe) return;

        Scoop(w);
    }

    void Scoop(Vector3 w)
    {
        Ingredient prefab = servingRecipe.cookedOutput;
        if (!prefab) return;

        w.z = prefab.transform.position.z;
        Ingredient scoop = Instantiate(prefab, w, Quaternion.identity);
        scoop.discardIfRejected = true;   // a scoop dropped in the void is lost
        scoop.GetComponent<Draggable>().BeginDrag();

        servingsLeft--;
        Log($"scooped, {servingsLeft} left");
        if (servingsLeft <= 0) Clear();   // pot is empty
    }

    // ---------- reset ----------

    /// Back to empty, ready for the next batch.
    public void Clear()
    {
        StopAllCoroutines();
        tracker.Clear();
        visuals.Clear();
        readyRecipe = null;
        servingRecipe = null;
        servingsLeft = 0;
        serving = false;
        cooking = false;
        if (emptySprite) vesselRenderer.sprite = emptySprite;
        if (progressBar) progressBar.SetActive(false);
        if (steamEffect) steamEffect.SetActive(false);
    }

    void SetProgress(float p)
    {
        if (!fillPivot) return;
        p = Mathf.Clamp01(p);
        float width = fillWidth * p;
        fillPivot.localScale = new Vector3(width, fillScale.y, fillScale.z);
        fillPivot.localPosition = new Vector3(-(fillWidth - width) / 2f,
            fillPivot.localPosition.y, fillPivot.localPosition.z);
    }

    void Log(string msg)
    {
        if (debugLogs) Debug.Log($"[Vessel {name}] {msg}", this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}
