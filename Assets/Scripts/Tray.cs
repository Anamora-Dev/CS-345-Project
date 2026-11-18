using UnityEngine;
using UnityEngine.InputSystem;

/// Sheet pan / baking tray. A draggable that holds loose ingredients in slots (no required order).
///  - Drop an ingredient on it and it snaps into the next free slot and rides along when you carry the tray.
///  - Drag an item off the tray (pull it past Leave Distance) to take it back out, e.g. onto a plate.
///  - Drop the tray on an Oven to cook everything on it.
[RequireComponent(typeof(Draggable))]
public class Tray : MonoBehaviour, IDropTarget
{
    [SerializeField] Transform[] slots;            // empty children marking where items sit
    [SerializeField] float leaveDistance = 0.4f;   // how far an item must be pulled to count as taken off
    [SerializeField] bool debugLogs;

    Draggable drag;
    Camera cam;
    Ingredient[] items;
    bool locked;   // true while cooking: nothing can be moved
    public Ingredient[] Items => items;

    void Awake()
    {
        drag = GetComponent<Draggable>();
        cam = Camera.main;
        items = new Ingredient[slots.Length];
    }

    void OnEnable() => drag.Dropped += OnTrayDropped;
    void OnDisable() => drag.Dropped -= OnTrayDropped;
    

    // ---------- ingredients dropped onto the tray ----------

    public bool TryAccept(Ingredient item)
    {
        if (locked) return false;

        // Already on this tray (dragged a little, then let go): snap it back into its slot.
        int existing = System.Array.IndexOf(items, item);
        if (existing >= 0)
        {
            Place(existing, item);
            item.KeepAfterAccept();
            return true;
        }

        int free = System.Array.IndexOf(items, null);
        if (free < 0) return false;   // tray is full

        Place(free, item);
        item.KeepAfterAccept();       // the tray keeps the item instead of consuming it
        return true;
    }

    void Place(int slot, Ingredient item)
    {
        items[slot] = item;
        item.transform.SetParent(slots[slot], true);
        Vector3 p = slots[slot].position;
        p.z = item.transform.position.z;
        item.transform.position = p;

        // Keep a lower sorting order while sitting on the tray so it stays behind UI/dragging items
        var sr = item.GetComponent<SpriteRenderer>();
        if (sr) sr.sortingOrder -= 5;
    }

    // ---------- used by the Oven ----------

    public bool HasCookable
    {
        get
        {
            foreach (var it in items)
                if (it != null && it.data != null && it.data.ResultFor(ProcessAction.Bake) != null) return true;
            return false;
        }
    }


    public bool IsLocked => locked;

    public void SetLocked(bool value) => locked = value;

    /// World-space center of the tray's collider. This is where the tray visually is,
    /// even when the root object's pivot is somewhere else.
    public Vector3 Center => drag.Col.bounds.center;

    /// Applies Cook to every item on the tray. Items with no Cook transformation are left as they are.
    public void CookAll()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;
            Ingredient cooked = items[i].Apply(ProcessAction.Bake);
            if (cooked != null) Place(i, cooked);
        }
    }

    /// Replaces the item in a single slot with its cooked prefab, keeping its slot position.
    public void ReplaceSlot(int slot, Ingredient cookedPrefab)
    {
        if (slot < 0 || slot >= items.Length) return;

        if (items[slot] != null)
        {
            Destroy(items[slot].gameObject);
            items[slot] = null;
        }

        if (cookedPrefab != null)
        {
            Ingredient cookedInstance = Instantiate(cookedPrefab);
            Place(slot, cookedInstance);
            cookedInstance.KeepAfterAccept();
        }
    }
    
    /// Clears current items and places a single cooked recipe output into slot 0.
    public void ReplaceWithCooked(Ingredient cookedPrefab)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                Destroy(items[i].gameObject);
                items[i] = null;
            }
        }

        if (cookedPrefab != null && slots.Length > 0)
        {
            Ingredient cookedInstance = Instantiate(cookedPrefab);
            Place(0, cookedInstance);
            cookedInstance.KeepAfterAccept();
        }
    }

    void Update()
    {
        for (int i = 0; i < items.Length; i++)
        {
            Ingredient it = items[i];
            if (it == null) { items[i] = null; continue; }   // used up elsewhere (e.g. served)

            Draggable d = it.GetComponent<Draggable>();

            if (!locked && Vector2.Distance(it.transform.position, slots[i].position) > leaveDistance)
            {
                // Pulled off the tray.
                it.transform.SetParent(null, true);
                items[i] = null;
                d.canDrag = true;

                // Restore sorting order
                var sr = it.GetComponent<SpriteRenderer>();
                if (sr) sr.sortingOrder += 5;
                continue;
            }

            d.canDrag = !locked;
        }
    }

    void LateUpdate()
    {
        // Pressing on an item should pick up the item, not the whole tray.
        if (Mouse.current == null) return;
        Vector2 m = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        bool overItem = false;
        foreach (var it in items)
        {
            if (it == null) continue;
            Collider2D c = it.GetComponent<Collider2D>();
            if (c != null && c.OverlapPoint(m)) { overItem = true; break; }
        }
        drag.canDrag = !locked && !overItem;
    }

    // ---------- editor helper ----------

    /// Right-click the Tray component header > "Fit Collider To Sprites". Do it while the tray is empty.
    [ContextMenu("Fit Collider To Sprites")]
    void FitCollider()
    {
        var box = GetComponent<BoxCollider2D>();
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        if (box == null || renderers.Length == 0)
        {
            Debug.LogWarning("Tray needs a BoxCollider2D and at least one child SpriteRenderer.", this);
            return;
        }

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);

        Vector3 s = transform.lossyScale;
        box.offset = transform.InverseTransformPoint(b.center);
        box.size = new Vector2(b.size.x / Mathf.Abs(s.x), b.size.y / Mathf.Abs(s.y));
    }

    // ---------- the tray itself dropped ----------

    void OnTrayDropped(Draggable d)
    {
        // The tray only has to overlap the oven's drop zone, not have its center inside it.
        foreach (var oven in Oven.All)
        {
            if (!oven.Overlaps(drag.Col)) continue;
            if (oven.TryAccept(this)) return;
            if (debugLogs) Debug.Log("[Tray] oven overlapped but rejected the tray (already holding one, or no snap point)", this);
        }

        if (debugLogs) Debug.Log("[Tray] dropped, but not overlapping any oven's collider", this);
        // Not over an oven: it just stays where you put it.
    }
}
