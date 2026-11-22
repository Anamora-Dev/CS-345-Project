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

    public TicketView SpawnRandomCustomerOrder(int minItems = 1, int maxItems = 3)
    {
        if (menuPool == null || menuPool.Count == 0 || ticketPrefab == null) return null;

        int count = Random.Range(Mathf.Max(1, minItems), Mathf.Max(minItems, maxItems) + 1);
        var picked = new List<Recipe>(count);
        for (int i = 0; i < count; i++)
            picked.Add(menuPool[Random.Range(0, menuPool.Count)]);

        return SpawnOrder(picked, $"Guest #{nextTicketNumber}");
    }

    public TicketView SpawnOrder(List<Recipe> recipes, string customerName = "Guest")
    {
        var order = new CustomerOrder
        {
            ticketNumber = nextTicketNumber++,
            customerName = customerName,
            recipes = new List<Recipe>(recipes)
        };
        order.EnsureStateArrays();

        var ticket = Instantiate(ticketPrefab, ticketParent);
        ticket.Setup(order);
        activeTickets[order.ticketNumber] = ticket;
        return ticket;
    }

    /// <summary>
    /// Call this when the player brings a cooked Recipe to a customer/ticket.
    /// Strikes through the first matching uncompleted item on that ticket.
    /// </summary>
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