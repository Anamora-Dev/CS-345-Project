using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class EggCarton : MonoBehaviour
{
    public Draggable eggPrefab;
    public Pan pan;
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
        w.z = eggPrefab.transform.position.z;
        if (Physics2D.OverlapPoint(w) != col) return;

        Draggable egg = Instantiate(eggPrefab, w, Quaternion.identity);
        egg.Dropped += OnEggDropped;
        egg.BeginDrag();
    }

    void OnEggDropped(Draggable egg)
    {
        egg.Dropped -= OnEggDropped;
        if (!pan.TryCook(egg))
            Destroy(egg.gameObject);   // dropped somewhere else, or pan busy
    }
}