using UnityEngine;
using UnityEngine.InputSystem;

public class CabinetDoor : MonoBehaviour
{
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private GameObject openVisual;

    [SerializeField] private Collider2D closedCollider;
    [SerializeField] private Collider2D openCollider;

    private Camera cam;
    private bool isOpen;

    public bool IsOpen => isOpen;
    public bool JustOpened { get; private set; }

    void Awake()
    {
        cam = Camera.main;
        SetOpen(false);
    }

    void Update()
    {
        JustOpened = false;

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        Collider2D activeCollider = isOpen ? openCollider : closedCollider;

        if (activeCollider != null && activeCollider.OverlapPoint(mouseWorld))
        {
            bool opening = !isOpen;
            SetOpen(!isOpen);

            if (opening)
                JustOpened = true;
        }
    }

    void SetOpen(bool open)
    {
        isOpen = open;

        closedVisual.SetActive(!open);
        openVisual.SetActive(open);

        closedCollider.enabled = !open;
        openCollider.enabled = open;
    }
}