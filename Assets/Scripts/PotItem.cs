using UnityEngine;

public enum IngredientType { None, Lid, Tomato, Basil }

/// Put this on the LID, the TOMATO and the BASIL (next to their Draggable).
/// Set the type and the pot in the inspector. No changes needed to Draggable.
[RequireComponent(typeof(Draggable))]
public class PotItem : MonoBehaviour
{
    [SerializeField] private IngredientType type = IngredientType.Tomato;
    [SerializeField] private PotController pot;
    [SerializeField] private bool destroyIfRejected = false;   // tick for spawned items (e.g. basil from the plant)

    public PotController Pot { get => pot; set => pot = value; }

    private Draggable draggable;

    private void Awake()
    {
        draggable = GetComponent<Draggable>();
    }

    private void OnEnable()  { draggable.Dropped += OnDropped; }
    private void OnDisable() { draggable.Dropped -= OnDropped; }

    private void OnDropped(Draggable d)
    {
        if (type == IngredientType.Lid)
        {
            pot.OnLidDropped();
            return;
        }

        bool accepted = pot.OnIngredientDropped(type, transform.position);
        if (accepted)
            gameObject.SetActive(false);                    // used up
        else if (destroyIfRejected)
            Destroy(gameObject);                            // spawned copy, just discard it
        else
            transform.position = d.HomePosition;            // send it back
    }
}