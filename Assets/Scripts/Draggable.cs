using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Draggable : MonoBehaviour
{
    public event Action<Draggable> Dropped;
    public static event Action<Draggable> AnyDropped;
    public bool canDrag = true;

    Camera cam;
    Collider2D col;
    SpriteRenderer sr;
    Vector3 grabOffset;
    int baseSortingOrder;
    bool dragging;

    public Vector3 HomePosition { get; set; }
    public Vector3 originalScale;
    public Collider2D Col => col;

    void Awake()
    {
        cam = Camera.main;
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        HomePosition = transform.position;
        originalScale = transform.localScale;
        if (sr) baseSortingOrder = sr.sortingOrder;
    }

    Vector3 MouseWorld()
    {
        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 w = cam.ScreenToWorldPoint(screen);
        w.z = transform.position.z;
        return w;
    }

    public void BeginDrag()
    {
        dragging = true;
        grabOffset = Vector3.zero;
        if (sr) sr.sortingOrder = baseSortingOrder + 10;
    }

    bool HitsMe(Vector3 p)
    {
        foreach (Collider2D c in Physics2D.OverlapPointAll(p))
            if (c == col) return true;
        return false;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (canDrag && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (HitsMe(MouseWorld()))
            {
                dragging = true;
                grabOffset = transform.position - MouseWorld();
                if (sr) sr.sortingOrder = baseSortingOrder + 10;
            }
        }

        if (dragging)
        {
            transform.position = MouseWorld() + grabOffset;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
                if (sr) sr.sortingOrder = baseSortingOrder;
                Dropped?.Invoke(this);
                Dropped?.Invoke(this);
                AnyDropped?.Invoke(this);   
            }
        }
    }
}