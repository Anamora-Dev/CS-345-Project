using UnityEngine;
using UnityEngine.InputSystem;

/// One spawner for everything (fridge shelf, cabinet, basil plant, plate stack...).
/// Click it and a new item appears already attached to the cursor.
[RequireComponent(typeof(Collider2D))]
public class Spawner : MonoBehaviour
{
    [SerializeField] Ingredient prefab;

    [Tooltip("Optional: the door this spawner sits behind. Stops the click that opens the door from also spawning.")]
    [SerializeField] Door door;

    /// Can also be switched off from code.
    public bool interactable = true;

    Camera cam;
    Collider2D col;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!interactable || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (door != null && (!door.IsOpen || door.JustOpened)) return;

        Vector3 w = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        w.z = prefab.transform.position.z;
        if (!HitsMe(w)) return;
    
        Ingredient item = Instantiate(prefab, w, prefab.transform.rotation);
        item.transform.localScale = prefab.transform.localScale;
        item.discardIfRejected = true;   // a dropped-in-the-void copy disappears instead of piling up
        item.GetComponent<Draggable>().BeginDrag();
    }

    bool HitsMe(Vector3 p)
    {
        foreach (Collider2D c in Physics2D.OverlapPointAll(p))
            if (c == col) return true;
        return false;
    }
}
