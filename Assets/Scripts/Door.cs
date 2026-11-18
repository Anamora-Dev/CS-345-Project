using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;

    [SerializeField] private Collider2D closedCollider;
    [SerializeField] private Collider2D openCollider;

    [Tooltip("Spawners (or anything else) that should only exist while this door is open.")]
    [SerializeField] private GameObject[] contents;

    private Camera cam;
    private bool isOpen;
    private int openedFrame = -1;

    public bool IsOpen => isOpen;
    public event System.Action<bool> OpenChanged;

    /// True for the whole frame in which the door was opened, regardless of script execution order.
    /// Spawners use it so the click that opens the door doesn't also grab an item.
    public bool JustOpened => openedFrame == Time.frameCount;

    void Awake()
    {
        cam = Camera.main;
        SetOpen(false);
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        Collider2D activeCollider = isOpen ? openCollider : closedCollider;

        if (activeCollider != null && activeCollider.OverlapPoint(mouseWorld))
        {
            bool opening = !isOpen;
            SetOpen(opening);

            if (opening)
                openedFrame = Time.frameCount;
        }
    }

    void SetOpen(bool open)
    {
        isOpen = open;

        closedVisual.SetActive(!open);
        openVisual.SetActive(open);

        closedCollider.enabled = !open;
        openCollider.enabled = open;

        foreach (var item in contents)
        {
            if (item)
                item.SetActive(open);
        }

        OpenChanged?.Invoke(open);
    }
}
