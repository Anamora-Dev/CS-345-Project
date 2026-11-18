using UnityEngine;
using UnityEngine.InputSystem;

public abstract class DishSpawner<TDish, TDesign> : MonoBehaviour
    where TDish : MonoBehaviour
    where TDesign : ScriptableObject
{
    [Header("Spawner Settings")]
    [SerializeField] protected TDish prefab;
    [SerializeField] protected TDesign[] designs;
    [SerializeField] protected bool randomDesign = true;

    Collider2D col;
    Camera cam;
    int next;

    protected virtual void Awake()
    {
        col = GetComponent<Collider2D>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3 pos = cam.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        pos.z = prefab.transform.position.z;

        if (!HitsMe(pos))
            return;

        Spawn(pos);
    }

    bool HitsMe(Vector3 pos)
    {
        foreach (Collider2D hit in Physics2D.OverlapPointAll(pos))
        {
            if (hit == col)
                return true;
        }

        return false;
    }

    void Spawn(Vector3 pos)
    {
        if (prefab == null || designs == null || designs.Length == 0)
        {
            Debug.LogWarning("Spawner is missing a prefab or designs.", this);
            return;
        }

        TDesign design = randomDesign
            ? designs[Random.Range(0, designs.Length)]
            : designs[next++ % designs.Length];

        TDish dish = Instantiate(prefab, pos, Quaternion.identity);

        InitializeDish(dish, design);

        Draggable draggable = dish.GetComponent<Draggable>();

        if (draggable)
            draggable.BeginDrag();
        else
            Debug.LogWarning("Spawned dish needs a Draggable component.", dish);
    }

    protected abstract void InitializeDish(TDish dish, TDesign design);
}