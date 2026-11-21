using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketView : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Image headerImage;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Sprite collapsedSprite;
    [SerializeField] private Sprite expandedSprite;

    [Header("Header info (always visible)")]
    [SerializeField] private TMP_Text ticketNumberText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text itemCountText;

    [Header("Timer Urgency Colors")]
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningOrangeColor = new(1f, 0.55f, 0f);
    [SerializeField] private Color expiredRedColor = new(0.9f, 0.15f, 0.15f);
    [Tooltip("Share of total expected time remaining when timer turns orange (0.25 = last 25%).")]
    [Range(0.05f, 0.75f)]
    [SerializeField] private float warningThresholdFraction = 0.25f;

    [Header("Expanded body")]
    [SerializeField] private RectTransform body;
    [SerializeField] private TicketLineView linePrefab;
    [SerializeField] private GameObject endCapPrefab;

    private readonly List<TicketLineView> spawnedLines = new();
    private CustomerOrder currentOrder;
    private float totalExpectedTime;
    private float startTime;
    private int lastShownSeconds = -1;
    private bool expanded;
    private bool expiredTriggered;

    public int TicketNumber => currentOrder != null ? currentOrder.ticketNumber : -1;
    public bool IsExpired => currentOrder == null || currentOrder.isExpired;
    public float ElapsedSeconds => Time.time - startTime;

    private void Awake()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(Toggle);
        SetExpanded(false);
    }

    public void Setup(CustomerOrder order)
    {
        currentOrder = order;
        expiredTriggered = false;
        lastShownSeconds = -1;
        startTime = Time.time;

        if (currentOrder == null)
        {
            totalExpectedTime = 0f;
            if (ticketNumberText != null) ticketNumberText.text = "#---";
            if (itemCountText != null) itemCountText.text = "0 items";
            ClearBody();
            UpdateTimerVisuals(0f);
            return;
        }

        currentOrder.EnsureStateArrays();
        totalExpectedTime = currentOrder.TotalExpectedTime;

        if (ticketNumberText != null)
            ticketNumberText.text = $"#{currentOrder.ticketNumber:000}";

        int count = currentOrder.recipes != null ? currentOrder.recipes.Count : 0;
        if (itemCountText != null)
            itemCountText.text = count == 1 ? "1 item" : $"{count} items";

        RebuildLines();
        UpdateTimerVisuals(0f);
        Rebuild();
    }

    // Backwards-compatible overload if passing raw recipes directly
    public void Setup(int ticketNumber, IReadOnlyList<Recipe> recipes)
    {
        var order = new CustomerOrder
        {
            ticketNumber = ticketNumber,
            recipes = recipes != null ? new List<Recipe>(recipes) : new List<Recipe>()
        };
        Setup(order);
    }

    public bool TryMarkItemComplete(Recipe deliveredRecipe)
    {
        if (currentOrder == null || currentOrder.isExpired || deliveredRecipe == null)
            return false;

        currentOrder.EnsureStateArrays();
        for (int i = 0; i < currentOrder.recipes.Count; i++)
        {
            if (!currentOrder.itemCompleted[i] && currentOrder.recipes[i] == deliveredRecipe)
            {
                currentOrder.itemCompleted[i] = true;
                if (i < spawnedLines.Count)
                    spawnedLines[i].Setup(currentOrder.recipes[i], true);
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        if (currentOrder == null) return;

        float elapsed = ElapsedSeconds;
        int secs = Mathf.FloorToInt(elapsed);
        if (secs != lastShownSeconds)
        {
            lastShownSeconds = secs;
            if (timerText != null)
                timerText.text = $"{secs / 60:00}:{secs % 60:00}";
        }

        UpdateTimerVisuals(elapsed);
    }

    private void UpdateTimerVisuals(float elapsed)
    {
        if (totalExpectedTime > 0f && elapsed > totalExpectedTime)
        {
            if (timerText != null) timerText.color = expiredRedColor;

            if (!expiredTriggered)
            {
                expiredTriggered = true;
                currentOrder.isExpired = true;
                CustomerManager.Instance?.OnCustomerWalkedOut(currentOrder);
                // Ticket order is now void/expired until dragged to the spike
                currentOrder = null;
            }
            return;
        }

        float remaining = totalExpectedTime - elapsed;
        bool inWarningWindow = totalExpectedTime > 0f &&
                               remaining <= totalExpectedTime * warningThresholdFraction;

        if (timerText != null)
            timerText.color = inWarningWindow ? warningOrangeColor : normalTimerColor;
    }

    private void RebuildLines()
    {
        ClearBody();
        if (currentOrder == null || currentOrder.recipes == null) return;

        for (int i = 0; i < currentOrder.recipes.Count; i++)
        {
            var line = Instantiate(linePrefab, body);
            bool done = currentOrder.itemCompleted != null &&
                        i < currentOrder.itemCompleted.Length &&
                        currentOrder.itemCompleted[i];
            line.Setup(currentOrder.recipes[i], done);
            spawnedLines.Add(line);
        }

        if (endCapPrefab != null)
            Instantiate(endCapPrefab, body);
    }

    private void ClearBody()
    {
        spawnedLines.Clear();
        if (body == null) return;
        for (int i = body.childCount - 1; i >= 0; i--)
            Destroy(body.GetChild(i).gameObject);
    }

    public void Toggle() => SetExpanded(!expanded);

    public void SetExpanded(bool value)
    {
        expanded = value;
        if (headerImage != null)
        {
            headerImage.sprite = value ? expandedSprite : collapsedSprite;
            headerImage.SetNativeSize();
        }
        if (body != null)
            body.gameObject.SetActive(value);
        Rebuild();
    }

    private void OnDestroy()
    {
        CustomerManager.Instance?.UnregisterTicket(this);
    }

    private void Rebuild() =>
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
}