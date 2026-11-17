using System.Collections;
using UnityEngine;

/// Put this on the POT. Drives the whole recipe:
/// lid off -> tomato -> basil -> lid back on -> cooks.
public class PotController : MonoBehaviour
{
    public enum PotState { Empty, HasTomato, HasBasil, Cooking, Done }

    [Header("References")]
    [SerializeField] private SpriteRenderer potRenderer;   // the pot's SpriteRenderer
    [SerializeField] private Transform lid;                // the lid object
    [SerializeField] private Transform lidSnapPoint;       // child of the lid

    [Header("Sprites")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite tomatoSauceSprite;
    [SerializeField] private Sprite sauceWithBasilSprite;
    [SerializeField] private Sprite cookedSprite;

    [Header("Tuning")]
    [SerializeField] private float snapDistance = 1f;      // how close the lid must be to snap on
    [SerializeField] private float ingredientDropRadius = 1.2f; // how close an ingredient must be to count
    [SerializeField] private float cookTime = 3f;
    [SerializeField] private GameObject steamEffect;       // optional, shown while cooking

    public PotState State { get; private set; } = PotState.Empty;

    private void Awake()
    {
        if (potRenderer == null) potRenderer = GetComponent<SpriteRenderer>();
        potRenderer.sprite = emptySprite;
        if (steamEffect != null) steamEffect.SetActive(false);
    }

    /// True when the lid's snap point is sitting on the pot.
    public bool IsLidOn =>
        Vector2.Distance(lidSnapPoint.position, transform.position) <= snapDistance;

    // ---------- called by PotItem when the player releases something ----------

    /// Lid released. Snaps it on if close enough, and starts cooking if the recipe is ready.
    public void OnLidDropped()
    {
        if (!IsLidOn) return;

        // Move the lid so its snap point lands exactly on the pot.
        Vector3 offset = transform.position - lidSnapPoint.position;
        lid.position += new Vector3(offset.x, offset.y, 0f);

        if (State == PotState.HasBasil)
            StartCoroutine(Cook());
    }

    /// Ingredient released. Returns true if the pot accepted it (item should be consumed).
    public bool OnIngredientDropped(IngredientType type, Vector3 dropPosition)
    {
        if (State == PotState.Cooking || State == PotState.Done) return false;
        if (IsLidOn) return false;   // lid must be off first
        if (Vector2.Distance(dropPosition, transform.position) > ingredientDropRadius) return false;

        if (State == PotState.Empty && type == IngredientType.Tomato)
        {
            State = PotState.HasTomato;
            potRenderer.sprite = tomatoSauceSprite;
            return true;
        }

        if (State == PotState.HasTomato && type == IngredientType.Basil)
        {
            State = PotState.HasBasil;
            potRenderer.sprite = sauceWithBasilSprite;
            return true;
        }

        return false; // wrong order / wrong ingredient
    }

    private IEnumerator Cook()
    {
        State = PotState.Cooking;
        if (steamEffect != null) steamEffect.SetActive(true);

        yield return new WaitForSeconds(cookTime);

        potRenderer.sprite = cookedSprite;
        if (steamEffect != null) steamEffect.SetActive(false);
        State = PotState.Done;
    }

    /// Optional: call to reset the pot for the next order.
    public void ResetPot()
    {
        StopAllCoroutines();
        State = PotState.Empty;
        potRenderer.sprite = emptySprite;
        if (steamEffect != null) steamEffect.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, ingredientDropRadius);
    }
}
