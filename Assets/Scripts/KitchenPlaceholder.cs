using TMPro;
using UnityEngine;

// Temporary script
public class KitchenPlaceholder : MonoBehaviour
{
    [SerializeField] TMP_Text orderLabel;
    [SerializeField, Range(0f, 1f)] float testQuality = 0.9f;

    void Start()
    {
        var order = GameManager.Instance.CurrentOrder;
        orderLabel.text = order != null ? "Make: " + order.dishName : "No order (test)";
    }

    // Hook to a button's OnClick
    public void FinishCorrect()
    {
        var order = GameManager.Instance.CurrentOrder;
        GameManager.Instance.FinishOrder(order != null ? order.dishName : "", testQuality);
    }

    // Hook to a second button's OnClick
    public void FinishWrong()
    {
        GameManager.Instance.FinishOrder("Wrong Dish", 0.5f);
    }
}