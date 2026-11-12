using UnityEngine;

public class CuttingBoard : MonoBehaviour
{
    public Transform snapPoint;
    public Draggable knife;
    public GameObject halvesPrefab;

    Collider2D col;
    Draggable currentAvo;
    GameObject currentHalves;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null) Debug.LogError("CuttingBoard needs a Collider2D on this object.", this);
        if (halvesPrefab == null) Debug.LogError("CuttingBoard has no Halves Prefab assigned.", this);
        if (knife == null) Debug.LogError("CuttingBoard has no Knife assigned.", this);
        if (knife != null)
        Debug.Log("Board is listening to: " + knife.name + " | in scene: " + knife.gameObject.scene.IsValid(), this);
    }

    void OnEnable()  { if (knife != null) knife.Dropped += OnKnifeDropped; }
    void OnDisable() { if (knife != null) knife.Dropped -= OnKnifeDropped; }

    public bool TryChop(Draggable avo)
    {
        if (currentAvo != null) return false;
        if (!avo.Col.bounds.Intersects(col.bounds)) return false;

        if (currentHalves != null) Destroy(currentHalves);

        currentAvo = avo;
        avo.canDrag = false;
        avo.transform.position = new Vector3(
            snapPoint.position.x, snapPoint.position.y, avo.transform.position.z);
        return true;
    }

    void OnKnifeDropped(Draggable k)
    {
        bool onBoard = currentAvo != null;
        bool hit = onBoard && k.Col.Distance(currentAvo.Col).isOverlapped;
        Debug.Log("Knife dropped | avocado on board: " + onBoard + " | overlapping: " + hit, this);

        if (hit) Cut();
        // knife stays where you dropped it
    }

    void Cut()
    {
        if (halvesPrefab == null)
        {
            Debug.LogError("Can't cut: Halves Prefab slot is empty.", this);
            return;
        }

        Vector3 pos = currentAvo.transform.position;
        Destroy(currentAvo.gameObject);
        currentAvo = null;

        currentHalves = Instantiate(halvesPrefab, pos, Quaternion.identity);
        currentHalves.SetActive(true);   // in case the prefab was saved inactive
    }
}