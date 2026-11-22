using UnityEngine;
using UnityEngine.EventSystems;

public class TicketSpike : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var ticket = eventData.pointerDrag.GetComponentInParent<TicketView>();
        if (ticket == null) return;

        CustomerManager.Instance?.UnregisterTicket(ticket);
        Destroy(ticket.gameObject);
    }
}