using UnityEngine;

[RequireComponent(typeof(Draggable))]
public class Ingredient : MonoBehaviour
{
    public IngredientData data;
    [SerializeField] bool debugLogs;
    [Header("Baked Visual Swap")]
    [SerializeField] Sprite cookedSprite; // Drag your 'food1' sprite here

    SpriteRenderer sr;


    /// Set by Spawner: a rejected copy is thrown away instead of returning to where it spawned.
    [HideInInspector] public bool discardIfRejected;

    Draggable drag;
    bool keepAfterAccept;

    void Awake() => drag = GetComponent<Draggable>();
    void OnEnable() => drag.Dropped += OnDropped;
    void OnDisable() => drag.Dropped -= OnDropped;

    /// A drop target calls this from TryAccept when it wants to hold the item instead of consuming it.
    public void KeepAfterAccept() => keepAfterAccept = true;

    void OnDropped(Draggable d)
    {
        foreach (var hit in Physics2D.OverlapPointAll(transform.position))
        {
            if (hit.transform.IsChildOf(transform)) continue;

            var target = hit.GetComponentInParent<IDropTarget>();
            if (target == null) continue;

            if (target.TryAccept(this))
            {
                if (keepAfterAccept) keepAfterAccept = false;
                else Destroy(gameObject);
                return;
            }
            if (debugLogs) Debug.Log($"[Ingredient] {hit.name} rejected {name}");
        }

        Reject();
    }

    void Reject()
    {
        if (discardIfRejected) Destroy(gameObject);
        else transform.position = drag.HomePosition;
    }

    /// Turn this item into its transformed version (chop, unwrap...).
    /// Returns the new item, or null if this item has no transformation for the action.
    /// With keepDragging, the new item continues the current mouse drag.
    public Ingredient Apply(ProcessAction action, bool keepDragging = false)
    {
        Ingredient result = data ? data.ResultFor(action) : null;
        if (!result) return null;

        Vector3 pos = transform.position;
        pos.z = result.transform.position.z;
        Ingredient next = Instantiate(result, pos, Quaternion.identity);
        next.discardIfRejected = discardIfRejected;

        // Make sure this one can't react to anything else before it's destroyed.
        drag.enabled = false;
        enabled = false;
        Destroy(gameObject);

        if (keepDragging) next.GetComponent<Draggable>().BeginDrag();
        return next;
    }
}
