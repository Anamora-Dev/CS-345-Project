using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Avocado : MonoBehaviour
{
    public Draggable avoPrefab;
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
        w.z = avoPrefab.transform.position.z;
        if (Physics2D.OverlapPoint(w) != col) return;

        Draggable avo = Instantiate(avoPrefab, w, Quaternion.identity);
        avo.Dropped += OnAvoDropped;
        avo.BeginDrag();
    }

    void OnAvoDropped(Draggable avo)
    {
        avo.Dropped -= OnAvoDropped;
        if (!cuttingBoard.TryChop(avo))
            Destroy(avo.gameObject);   // dropped somewhere else, or board busy
    }
}