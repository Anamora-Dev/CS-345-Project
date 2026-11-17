
using UnityEngine;
using UnityEngine.InputSystem;

public class TrashCan : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite emptySprite;
    public Sprite halfFullSprite;
    public Sprite fullSprite;

    [Header("Trash Settings")]
    public int itemsForHalfFull = 2;
    public int itemsForFull = 5;

    [Header("UI")]
    public GameObject trashPopup;
    public GameObject fullIndicator;

    [Header("Popup Position")]
    public Vector3 popupOffset = new Vector3(0, 1.5f, 0);

    private int trashCount = 0;

    private SpriteRenderer sr;
    private Collider2D trashCollider;
    private Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        trashCollider = GetComponent<Collider2D>();
        cam = Camera.main;

        trashPopup.SetActive(false);

        if (fullIndicator != null)
            fullIndicator.SetActive(false);

        UpdateSprite();
    }

    void OnEnable()
    {
        Draggable.AnyDropped += OnItemDropped;
    }

    void OnDisable()
    {
        Draggable.AnyDropped -= OnItemDropped;
    }

    void Update()
    {
        if (Mouse.current == null || cam == null)
            return;

        // Only allow clicking when the trash is full
        if (trashCount < itemsForFull)
            return;

        // Check for mouse click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition =
                cam.ScreenToWorldPoint(
                    Mouse.current.position.ReadValue()
                );
        }
    }

    void OnItemDropped(Draggable item)
    {
        if (item == null || !item.canBeTrashed)
            return;

        // Don't accept more trash when full
        if (trashCount >= itemsForFull)
            return;

        // Check if item was dropped inside trash can
        if (!trashCollider.OverlapPoint(item.Col.bounds.center))
            return;

        trashCount++;

        UpdateSprite();

        Destroy(item.gameObject);
    }

    void UpdateSprite()
    {
        if (trashCount >= itemsForFull)
        {
            sr.sprite = fullSprite;

            if (fullIndicator != null)
                fullIndicator.SetActive(true);
                OpenTrashPopup();
        }
        else if (trashCount >= itemsForHalfFull)
        {
            sr.sprite = halfFullSprite;
        }
        else
        {
            sr.sprite = emptySprite;
        }
    }

    // Open popup above trash can
    public void OpenTrashPopup()
    {
        PositionPopup();
        trashPopup.SetActive(true);
    }

    // Position popup relative to trash can
    void PositionPopup()
    {
        Vector3 screenPosition = cam.WorldToScreenPoint(
            transform.position + popupOffset
        );

        trashPopup.transform.position = screenPosition;
    }

    // Close popup without emptying trash
    public void CloseTrashPopup()
    {
        trashPopup.SetActive(false);
    }

    // Take out trash and reset everything
    public void TakeOutTrash()
    {
        trashCount = 0;

        UpdateSprite();

        if (fullIndicator != null)
            fullIndicator.SetActive(false);

        trashPopup.SetActive(false);
    }
}
