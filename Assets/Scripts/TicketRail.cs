using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The TicketHolder object will use this script
public class TicketRail : MonoBehaviour
{
    public GameObject ticketPrefab;   // SampleTicket
    public ReceiptUI receiptUI;       // ReceiptManager

    readonly Dictionary<int, GameObject> tickets = new Dictionary<int, GameObject>();

    public bool HasTicket(int slot)
    {
        return tickets.ContainsKey(slot) && tickets[slot] != null;
    }

    public void AddTicket(int slot, Customers customers, CustomerBehavior customer)
    {
        if (HasTicket(slot)) return;

        GameObject ticket = Instantiate(ticketPrefab, transform);
        tickets[slot] = ticket;

        // fill in the text (first TMP text inside the prefab)
        TMP_Text label = ticket.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.raycastTarget = false;
            var sb = new StringBuilder();
            sb.AppendLine("#" + customer.orderNumber.ToString("0000"));
            foreach (int item in customer.orderItems)
                sb.AppendLine(customers.GetFoodName(item));
            label.text = sb.ToString();
        }

        // click opens the receipt
        Button button = ticket.GetComponent<Button>();
        if (button == null) button = ticket.AddComponent<Button>();
        button.onClick.AddListener(() => receiptUI.Show(customers, customer));
    }

    public void RemoveTicket(int slot)
    {
        if (!tickets.ContainsKey(slot)) return;
        if (tickets[slot] != null) Destroy(tickets[slot]);
        tickets.Remove(slot);
    }
}