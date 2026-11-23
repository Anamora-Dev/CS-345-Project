using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomerView : MonoBehaviour, IPointerClickHandler
{
    [Header("Customer & Bubble")]
    [SerializeField] private Image customerSquare;       // square sprite for the customer
    [SerializeField] private RectTransform speechBubble;  // square speech bubble container
    [SerializeField] private Image itemIconPrefab;        // simple Image prefab for each item sprite

    public event Action<CustomerView> Clicked;

    public CustomerOrder Order { get; private set; }
    public bool TicketTaken { get; set; }

    private readonly List<Image> spawnedIcons = new();

    public void Setup(CustomerOrder order, Sprite customerSprite = null)
    {
        Order = order;
        TicketTaken = false;

        if (customerSquare != null && customerSprite != null)
            customerSquare.sprite = customerSprite;

        RebuildSpeechBubble();
    }

    private void RebuildSpeechBubble()
    {
        foreach (var img in spawnedIcons)
        {
            if (img != null) Destroy(img.gameObject);
        }
        spawnedIcons.Clear();

        if (speechBubble == null || itemIconPrefab == null || Order?.recipes == null)
            return;

        foreach (var recipe in Order.recipes)
        {
            if (recipe == null) continue;
            var icon = Instantiate(itemIconPrefab, speechBubble);
            icon.sprite = recipe.ticketIcon;
            icon.enabled = recipe.ticketIcon != null;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            spawnedIcons.Add(icon);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (TicketTaken || Order == null || Order.isExpired) return;
        Clicked?.Invoke(this);
    }
}