using UnityEngine;
using UnityEngine.EventSystems;

/// Added to a customer's ChatBubble at runtime (by Customers.cs).
/// Clicking the bubble, or the food pictures inside it, opens that customer's receipt.
/// This version prints messages to the Console so you can see why a click isn't working.
public class OrderBubble : MonoBehaviour, IPointerClickHandler
{
    CustomerBehavior customer;
    Customers customers;

    public void Setup(CustomerBehavior customer, Customers customers)
    {
        this.customer = customer;
        this.customers = customers;
        Debug.Log("OrderBubble ready on " + name + " (order #" + customer.orderNumber + ")");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OrderBubble CLICKED");

        if (customers == null) { Debug.LogError("OrderBubble: Customers reference is missing."); return; }
        if (customers.receiptUI == null) { Debug.LogError("OrderBubble: 'Receipt UI' is empty on the Customers script. Drag ReceiptManager into it."); return; }
        if (customer == null) { Debug.LogError("OrderBubble: customer reference is missing."); return; }

        customers.receiptUI.Show(customers, customer);
    }
}