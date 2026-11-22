using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Builds and shows a receipt for a customer's order:
/// order number, each food item with its price, subtotal, tax, and total.
/// Put this on an always-active empty object (not on the panel itself).
public class ReceiptUI : MonoBehaviour
{
    [SerializeField] GameObject panel;        // the receipt panel (starts hidden)
    [SerializeField] TMP_Text receiptText;    // one text object that holds the whole receipt
    [SerializeField] Button closeButton;      // optional close button
    [SerializeField] float taxRate = 0.05f;   // 0.05 = 5%. Change this to whatever tax you want.

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    public void Show(Customers customers, CustomerBehavior customer)
    {
        if (customer == null || customer.orderItems == null || customer.orderItems.Length == 0) return;

        float subtotal = 0f;
        var sb = new StringBuilder();

        sb.AppendLine("<b>RECEIPT</b>");
        sb.AppendLine("Order #" + customer.orderNumber.ToString("0000"));
        sb.AppendLine();

        foreach (int itemIndex in customer.orderItems)
        {
            string name = customers.GetFoodName(itemIndex);
            float price = customers.GetFoodPrice(itemIndex);
            subtotal += price;
            sb.AppendLine(name + "<pos=72%>" + Money(price));   // lines the prices up in a column
        }

        float tax = Mathf.Round(subtotal * taxRate * 100f) / 100f;
        float total = subtotal + tax;

        sb.AppendLine();
        sb.AppendLine("Subtotal<pos=72%>" + Money(subtotal));
        sb.AppendLine("Tax (" + (taxRate * 100f).ToString("0.##", CultureInfo.InvariantCulture) + "%)<pos=72%>" + Money(tax));
        sb.AppendLine("<b>Total<pos=72%>" + Money(total) + "</b>");

        receiptText.text = sb.ToString();
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    static string Money(float value)
    {
        return "$" + value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}