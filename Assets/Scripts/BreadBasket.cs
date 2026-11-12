using UnityEngine;
using UnityEngine.InputSystem;

public class BreadBasket : MonoBehaviour
{
    public GameObject breadPrefab;
    public CabinetDoor cabinetDoor;
    public Vector3 spawnOffset = Vector3.zero;

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current == null || col == null)
            return;

        // Don't allow bread to spawn if cabinet is closed
        if (cabinetDoor == null || !cabinetDoor.IsOpen || cabinetDoor.JustOpened)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

            if (col.OverlapPoint(mousePosition))
                SpawnBread();
        }
    }

    void SpawnBread()
    {
        GameObject bread = Instantiate(
            breadPrefab,
            transform.position + spawnOffset,
            Quaternion.identity
        );

        Draggable draggable = bread.GetComponent<Draggable>();

        if (draggable != null)
            draggable.BeginDrag();
    }
}