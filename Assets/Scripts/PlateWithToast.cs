
using UnityEngine;

public class PlateWithToast : MonoBehaviour
{
    public GameObject avocadoToastPrefab;
    public Vector3 spawnOffset = new Vector3(-1f, 0f, 0f);

    private Collider2D col;
    private bool used;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (col == null)
            Debug.LogError("Toast plate needs a Collider2D!", this);

        if (avocadoToastPrefab == null)
            Debug.LogError("Avocado toast prefab is missing!", this);
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

        // Only accept cut avocado
        if (!item.CompareTag("CutAvocado"))
            return;

        if (item.Col == null)
            return;

        // Check if avocado overlaps the toast plate
        if (!item.Col.Distance(col).isOverlapped)
            return;

        used = true;
        
        // Spawn avocado toast
        Instantiate(
        avocadoToastPrefab,
        transform.position + new Vector3(-2.3f, 0.05f, 0f),
        transform.rotation
    );


        // Remove cut avocado and previous plate
        Destroy(item.gameObject);
        Destroy(gameObject);
    }
}
