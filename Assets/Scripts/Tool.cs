using UnityEngine;

/// The knife (or any handheld tool). Drop it so it overlaps the item on a Station and it applies its action.
[RequireComponent(typeof(Draggable))]
public class Tool : MonoBehaviour
{
    [SerializeField] ProcessAction action = ProcessAction.Chop;
    [SerializeField] bool returnHome = false;   // off = the tool stays where you drop it

    Draggable drag;

    void Awake() => drag = GetComponent<Draggable>();
    void OnEnable() => drag.Dropped += OnDropped;
    void OnDisable() => drag.Dropped -= OnDropped;

    void OnDropped(Draggable d)
    {
        foreach (var station in Station.All)
            if (station.ApplyTool(action, drag.Col)) break;

        if (returnHome) transform.position = drag.HomePosition;
    }
}
