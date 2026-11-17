using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class BasilPlant : MonoBehaviour
{
    [SerializeField] private Draggable basilPrefab;   // prefab with Draggable + PotItem (type = Basil)
    [SerializeField] private PotController pot;       // scene object, handed to each spawned basil

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

        Vector3 w = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        w.z = basilPrefab.transform.position.z;

        if (!HitsPlant(w)) return;

        Draggable basil = Instantiate(basilPrefab, w, Quaternion.identity);

        PotItem item = basil.GetComponent<PotItem>();
        if (item != null) item.Pot = pot;   // prefabs can't reference scene objects, so set it here

        basil.BeginDrag();
    }

    bool HitsPlant(Vector3 p)
    {
        foreach (Collider2D c in Physics2D.OverlapPointAll(p))
            if (c == col) return true;
        return false;
    }
}
