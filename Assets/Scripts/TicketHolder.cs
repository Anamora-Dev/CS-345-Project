using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TicketHolder : MonoBehaviour, IDropHandler
{
    [SerializeField] private RectTransform ticketContainer; // has Horizontal Layout Group

    private readonly List<TicketView> heldTickets = new();

    private void Awake()
    {
        if (ticketContainer == null)
            ticketContainer = transform as RectTransform;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var ticket = eventData.pointerDrag.GetComponentInParent<TicketView>();
        if (ticket == null) return;

        SnapTicket(ticket);
    }

    public void SnapTicket(TicketView ticket)
    {
        if (ticket == null || ticketContainer == null) return;

        ticket.transform.SetParent(ticketContainer, false);
        ticket.transform.localScale = Vector3.one;

        var rt = ticket.transform as RectTransform;
        if (rt != null)
            rt.anchoredPosition = Vector2.zero;

        if (!heldTickets.Contains(ticket))
            heldTickets.Add(ticket);

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ticketContainer);
    }

    public void RemoveTicket(TicketView ticket)
    {
        if (ticket == null) return;
        heldTickets.Remove(ticket);
    }
}