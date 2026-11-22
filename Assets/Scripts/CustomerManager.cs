using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [SerializeField] private TicketView ticketPrefab;
    [SerializeField] private RectTransform ticketParent;
    [SerializeField] private List<Recipe> menuPool = new();
    [SerializeField] private int startingTicketNumber = 1;

    private readonly Dictionary<int, TicketView> activeTickets = new();
    private int nextTicketNumber;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        nextTicketNumber = startingTicketNumber;
    }

    // Builds an order from the menu pool without spawning a ticket yet.
    public CustomerOrder CreateRandomOrder(int minItems = 1, int maxItems = 3, string customerName = null)
    {
        if (menuPool == null || menuPool.Count == 0) return null;

        int count = Random.Range(Mathf.Max(1, minItems), Mathf.Max(minItems, maxItems) + 1);
        var picked = new List<Recipe>(count);
        for (int i = 0; i < count; i++)
            picked.Add(menuPool[Random.Range(0, menuPool.Count)]);

        return CreateOrder(picked, customerName ?? $"Guest #{nextTicketNumber}");
    }

    public CustomerOrder CreateOrder(List<Recipe> recipes, string customerName = "Guest")
    {
        var order = new CustomerOrder
        {
            ticketNumber = nextTicketNumber++,
            customerName = customerName,
            recipes = new List<Recipe>(recipes)
        };
        order.EnsureStateArrays();
        return order;
    }

    // Spawns the draggable ticket for an existing order, optionally at a screen position.
    public TicketView SpawnTicket(CustomerOrder order, Vector2? screenPosition = null)
    {
        if (order == null || ticketPrefab == null || ticketParent == null) return null;

        var ticket = Instantiate(ticketPrefab, ticketParent);
        ticket.Setup(order);
        activeTickets[order.ticketNumber] = ticket;

        if (screenPosition.HasValue)
            PlaceAtScreenPoint((RectTransform)ticket.transform, screenPosition.Value);

        return ticket;
    }

    private void PlaceAtScreenPoint(RectTransform ticketRect, Vector2 screenPoint)
    {
        Canvas canvas = ticketParent.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera
            : null;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(ticketParent, screenPoint, cam, out Vector3 world))
            ticketRect.position = world;
    }

    public TicketView SpawnOrder(List<Recipe> recipes, string customerName = "Guest")
        => SpawnTicket(CreateOrder(recipes, customerName));

    public TicketView SpawnRandomCustomerOrder(int minItems = 1, int maxItems = 3)
        => SpawnTicket(CreateRandomOrder(minItems, maxItems));

    public bool DeliverItemToTicket(int ticketNumber, Recipe deliveredRecipe)
    {
        if (!activeTickets.TryGetValue(ticketNumber, out var ticket) || ticket == null)
            return false;

        return ticket.TryMarkItemComplete(deliveredRecipe);
    }

    public void UnregisterTicket(TicketView ticket)
    {
        if (ticket == null) return;
        activeTickets.Remove(ticket.TicketNumber);
    }

    public void OnCustomerWalkedOut(CustomerOrder order)
    {
        // Hook for later: lower reputation/score, play walk-out cue, etc.
        Debug.Log($"Customer on ticket #{order.ticketNumber:000} left because time ran out.");
    }
}