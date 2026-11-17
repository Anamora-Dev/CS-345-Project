
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class BowlSpawner : MonoBehaviour
{
    [SerializeField] Bowl bowlPrefab;
    [SerializeField] BowlDesign[] designs;
    [SerializeField] bool randomDesign = true;

    Collider2D col;
    Camera cam;
    int next;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3 p = cam.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        p.z = bowlPrefab.transform.position.z;

        if (!HitsMe(p)) return;

        Spawn(p);
    }

    bool HitsMe(Vector3 p)
    {
        foreach (Collider2D c in Physics2D.OverlapPointAll(p))
        {
            if (c == col)
                return true;
        }

        return false;
    }

    void Spawn(Vector3 pos)
    {
        var design = randomDesign
            ? designs[Random.Range(0, designs.Length)]
            : designs[next++ % designs.Length];

        var bowl = Instantiate(
            bowlPrefab,
            pos,
            Quaternion.identity
        );

        bowl.Init(design);

        bowl.GetComponent<Draggable>().BeginDrag();
    }
}
