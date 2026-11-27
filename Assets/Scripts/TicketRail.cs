using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicketRail : MonoBehaviour
{
    public GameObject ticketPrefab;
    [SerializeField] private RectTransform ticketContainer;
    [SerializeField] private RectTransform pinContainer;

    private readonly Dictionary<int, GameObject> tickets = new Dictionary<int, GameObject>();
    private readonly Dictionary<TicketView, RectTransform> ticketPins =
        new Dictionary<TicketView, RectTransform>();
    private readonly List<RectTransform> railPins = new List<RectTransform>();

    private void Awake()
    {
        if (ticketContainer == null)
            ticketContainer = transform as RectTransform;

        if (pinContainer == null) return;

        for (int i = 0; i < pinContainer.childCount; i++)
        {
            RectTransform pin = pinContainer.GetChild(i) as RectTransform;
            Graphic pinGraphic = pin != null ? pin.GetComponent<Graphic>() : null;
            if (pinGraphic == null) continue;

            pinGraphic.raycastTarget = false;
            railPins.Add(pin);
        }
    }

    private void OnEnable()
    {
        Draggable.AnyDropped += HandleDroppedObject;
    }

    private void OnDisable()
    {
        Draggable.AnyDropped -= HandleDroppedObject;
    }

    public bool HasTicket(int slot)
    {
        return tickets.ContainsKey(slot) && tickets[slot] != null;
    }

    public bool AddTicket(int slot, Customers customers, CustomerBehavior customer)
    {
        if (HasTicket(slot)) return true;
        if (ticketPrefab == null || customers == null || customer == null)
        {
            Debug.LogError("TicketRail needs a ticket prefab, Customers reference, and customer order.");
            return false;
        }

        TicketView ticketView = ticketPrefab.GetComponent<TicketView>();
        if (ticketView == null)
        {
            Debug.LogError("TicketRail's ticket prefab must have a TicketView component.");
            return false;
        }

        if (customer.orderItems == null || customer.orderItems.Length == 0)
        {
            Debug.LogError($"Customer order #{customer.orderNumber:0000} has no items.");
            return false;
        }

        if (ticketContainer == null || pinContainer == null || railPins.Count == 0)
        {
            Debug.LogError("TicketRail needs a ticket drop area and at least one UI pin.");
            return false;
        }

        RectTransform pinForTicket = FindAvailablePin();
        if (pinForTicket == null)
        {
            Debug.LogWarning("No empty nail pins are available for another ticket.");
            return false;
        }

        GameObject ticket = Instantiate(ticketPrefab, pinContainer, false);
        RectTransform ticketRect = ticket.transform as RectTransform;
        ticketView = ticket.GetComponent<TicketView>();
        if (ticketRect == null ||
            !ticketView.SetupReceipt(customer.orderNumber, customers, customer.orderItems))
        {
            if (ticketRect == null)
                Debug.LogError("TicketRail's ticket prefab must have a RectTransform.");
            Destroy(ticket);
            return false;
        }

        tickets[slot] = ticket;
        ticketPins[ticketView] = pinForTicket;
        Canvas.ForceUpdateCanvases();
        PositionTicketAtPin(ticketRect, pinForTicket);
        pinForTicket.SetAsLastSibling();
        return true;
    }

    private RectTransform FindAvailablePin()
    {
        foreach (RectTransform pin in railPins)
        {
            if (!ticketPins.ContainsValue(pin))
                return pin;
        }
        return null;
    }

    public void RemoveTicket(int slot)
    {
        if (!tickets.ContainsKey(slot)) return;
        if (tickets[slot] != null)
        {
            TicketView ticketView = tickets[slot].GetComponent<TicketView>();
            if (ticketView != null)
                ticketPins.Remove(ticketView);
            Destroy(tickets[slot]);
        }
        tickets.Remove(slot);
    }

    private void HandleDroppedObject(Draggable draggable)
    {
        if (draggable == null || ticketContainer == null || pinContainer == null) return;

        TicketView ticket = draggable.GetComponent<TicketView>();
        if (ticket == null || !ticketPins.TryGetValue(ticket, out RectTransform assignedPin)) return;

        Canvas canvas = ticketContainer.GetComponentInParent<Canvas>();
        Canvas rootCanvas = canvas != null ? canvas.rootCanvas : null;
        Camera eventCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, ticket.transform.position);
        if (!RectTransformUtility.RectangleContainsScreenPoint(ticketContainer, screenPoint, eventCamera))
            return;

        ticket.transform.SetParent(pinContainer, false);
        RectTransform ticketRect = ticket.transform as RectTransform;
        if (ticketRect != null)
            PositionTicketAtPin(ticketRect, assignedPin);

        assignedPin.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
    }

    private static void PositionTicketAtPin(RectTransform ticket, RectTransform pin)
    {
        ticket.position = pin.TransformPoint(pin.rect.center);
    }
}