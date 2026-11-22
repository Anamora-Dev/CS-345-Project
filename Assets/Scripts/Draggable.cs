using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public event Action<Draggable> Dropped;
    public static event Action<Draggable> AnyDropped;

    public bool canDrag = true;
    public bool canBeTrashed = true;

    private bool isUI;
    private Camera cam;
    private Collider2D col;
    private SpriteRenderer sr;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    private Vector3 grabOffset;
    private int baseSortingOrder;
    private bool dragging;

    public Vector3 HomePosition { get; set; }
    public Vector3 originalScale;
    public Collider2D Col => col;

    private void Awake()
    {
        var parentCanvas = GetComponentInParent<Canvas>();
        rootCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;
        isUI = rootCanvas != null && (transform is RectTransform);

        cam = Camera.main;
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (isUI)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        else if (sr != null)
        {
            baseSortingOrder = sr.sortingOrder;
        }

        HomePosition = transform.position;
        originalScale = transform.localScale;
    }

    // --- Shared entry point ---

    public void BeginDrag()
    {
        if (!canDrag || dragging) return;
        dragging = true;

        if (isUI)
        {
            if (rootCanvas != null && transform.parent != rootCanvas.transform)
                transform.SetParent(rootCanvas.transform, true);

            transform.SetAsLastSibling();
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
        }
        else
        {
            if (sr != null)
                sr.sortingOrder = baseSortingOrder + 10;
        }
    }

    private void FinishDrag()
    {
        if (!dragging) return;
        dragging = false;

        if (isUI)
        {
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = true;
        }
        else
        {
            if (sr != null)
                sr.sortingOrder = baseSortingOrder;
        }

        Dropped?.Invoke(this);
        AnyDropped?.Invoke(this);
    }

    // --- UI Pointer Handlers (Tickets / Canvas objects) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isUI || !canDrag) return;

        BeginDrag();
        Vector3 pointerPos = GetUIPointerPos(eventData);
        grabOffset = transform.position - pointerPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isUI || !canDrag || !dragging) return;

        Vector3 pointerPos = GetUIPointerPos(eventData);
        transform.position = pointerPos + grabOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isUI || !dragging) return;
        FinishDrag();
    }

    private Vector3 GetUIPointerPos(PointerEventData eventData)
    {
        Vector2 screenPos = eventData != null
            ? eventData.position
            : (Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);

        if (rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return new Vector3(screenPos.x, screenPos.y, transform.position.z);

        Camera eventCam = eventData != null && eventData.pressEventCamera != null
            ? eventData.pressEventCamera
            : (rootCanvas != null && rootCanvas.worldCamera != null ? rootCanvas.worldCamera : cam);

        if (eventCam != null)
        {
            Vector3 world = eventCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, transform.position.z));
            world.z = transform.position.z;
            return world;
        }

        return new Vector3(screenPos.x, screenPos.y, transform.position.z);
    }

    // --- World Mouse Handlers (Knives / Food / Collider2D objects) ---

    private Vector3 MouseWorld()
    {
        if (cam == null) cam = Camera.main;
        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 w = cam.ScreenToWorldPoint(screen);
        w.z = transform.position.z;
        return w;
    }

    private bool HitsMe(Vector3 p)
    {
        if (col == null) return false;
        foreach (Collider2D c in Physics2D.OverlapPointAll(p))
            if (c == col) return true;
        return false;
    }

    private void Update()
    {
        if (isUI || Mouse.current == null) return;

        if (canDrag && !dragging && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 worldPoint = MouseWorld();
            if (HitsMe(worldPoint))
            {
                BeginDrag();
                grabOffset = transform.position - worldPoint;
            }
        }

        if (dragging)
        {
            transform.position = MouseWorld() + grabOffset;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                FinishDrag();
        }
    }
}