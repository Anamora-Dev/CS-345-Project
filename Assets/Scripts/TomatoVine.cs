using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class TomatoVine: MonoBehaviour
{
    public Draggable tomatoPrefab;
    public CuttingBoard cuttingBoard;
    public Door fridgeDoor;

    Camera cam;
    Collider2D col;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        if (fridgeDoor == null || !fridgeDoor.IsOpen || fridgeDoor.JustOpened)
            return;

        Vector3 w = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        w.z = tomatoPrefab.transform.position.z;
        if (Physics2D.OverlapPoint(w) != col) return;

        Draggable tomato = Instantiate(tomatoPrefab, w, Quaternion.identity);
        tomato.Dropped += OntomatoDropped;
        tomato.BeginDrag();
    }

    void OntomatoDropped(Draggable tomato)
    {
        tomato.Dropped -= OntomatoDropped;
        if (!cuttingBoard.TryChop(tomato))
            Destroy(tomato.gameObject);   // dropped somewhere else, or board busy
    }
}