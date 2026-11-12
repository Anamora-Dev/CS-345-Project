
using UnityEngine;

public class AvocadoToastEgg : MonoBehaviour
{
    public GameObject avocadoToastEggPrefab;

    private Collider2D col;
    private bool used;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (col == null)
            Debug.LogError("Avocado toast needs a Collider2D!", this);

        if (avocadoToastEggPrefab == null)
            Debug.LogError("Avocado toast with egg prefab is missing!", this);
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

        // Only accept cooked egg
        if (!item.CompareTag("CookedEgg"))
            return;

        if (item.Col == null)
            return;

        // Check if egg overlaps avocado toast
        if (!item.Col.Distance(col).isOverlapped)
            return;

        used = true;

        // Spawn avocado toast with egg
        Instantiate(
            avocadoToastEggPrefab,
            transform.position + new Vector3(2.3f, 0f, 0f),
            transform.rotation
        );

        // Remove cooked egg and previous toast
        Destroy(item.gameObject);
        Destroy(gameObject);
    }
}
