
using UnityEngine;

public class Plate : MonoBehaviour
{
    public GameObject plateWithToastPrefab;

    private Collider2D col;
    private bool used;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (col == null)
            Debug.LogError("Plate needs a Collider2D!", this);

        if (plateWithToastPrefab == null)
            Debug.LogError("Plate-with-toast prefab is missing!", this);
    }

    void OnEnable()
    {
        Draggable.AnyDropped += OnItemDropped;
    }

    void OnDisable()
    {
        Draggable.AnyDropped -= OnItemDropped;
    }

    void OnItemDropped(Draggable item)
    {
        if (used || item == null || col == null)
            return;

        // Only accept toasted bread
        if (!item.CompareTag("ToastedBread"))
            return;

        if (item.Col == null)
            return;

        // Check whether bread overlaps the plate
        if (!item.Col.Distance(col).isOverlapped)
            return;

        used = true;

        // Spawn plate with toast
        Instantiate(
            plateWithToastPrefab,
            transform.position,
            transform.rotation
        );

        // Remove original objects
        Destroy(item.gameObject);
        Destroy(gameObject);
    }
}
