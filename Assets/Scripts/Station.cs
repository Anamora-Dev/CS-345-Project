using System.Collections.Generic;
using UnityEngine;

/// Cutting board. An item dropped on it snaps to the snap point and stays there.
///  - A Tool (knife) dropped so it overlaps the held item applies its action (e.g. Chop).
///  - Dragging the held item off the board applies Unwrap (if the item has one) and carries on dragging.
public class Station : MonoBehaviour, IDropTarget
{
    public static readonly List<Station> All = new();

    [SerializeField] Transform snapPoint;
    [SerializeField] float leaveDistance = 0.3f;   // how far an item must be pulled to count as picked up

    Ingredient held;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public bool TryAccept(Ingredient item)
    {
        if (held != null) return false;                                  // board is occupied
        // Only things that can be chopped or unwrapped belong on the board (not, say, raw bacon).
        if (!item.data) return false;
        if (item.data.ResultFor(ProcessAction.Chop) == null &&
            item.data.ResultFor(ProcessAction.Unwrap) == null) return false;

        Hold(item);
        item.KeepAfterAccept();   // the board keeps the item instead of consuming it
        return true;
    }

    /// Called by a Tool when it is dropped. Only works if the tool overlaps the held item.
    public bool ApplyTool(ProcessAction action, Collider2D toolCollider)
    {
        if (held == null) return false;

        Collider2D heldCollider = held.GetComponent<Collider2D>();
        if (heldCollider == null || !toolCollider.Distance(heldCollider).isOverlapped) return false;

        Ingredient next = held.Apply(action);
        if (next == null) return false;   // e.g. still in its package

        Hold(next);
        return true;
    }

    void Update()
    {
        if (held == null) { held = null; return; }   // also clears it if the item was destroyed elsewhere

        if (Vector2.Distance(held.transform.position, snapPoint.position) <= leaveDistance) return;

        // Picked up off the board: take the package off and keep dragging the result.
        Ingredient leaving = held;
        held = null;
        leaving.Apply(ProcessAction.Unwrap, keepDragging: true);
    }

    void Hold(Ingredient item)
    {
        held = item;
        Vector3 p = snapPoint.position;
        p.z = item.transform.position.z;
        item.transform.position = p;
    }
}
