using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketLineView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text specsText;
    [SerializeField] private TMP_Text receiptPriceText;

    [Header("Backgrounds, shortest to longest")]
    [Tooltip("Element 0 = 0 specs (shortest), element 1 = 1 spec, element 2 = 2+ specs")]
    [SerializeField] private Sprite[] backgrounds;

    [SerializeField] private Image background;
    [SerializeField] private Color completedTextColor = new(0.45f, 0.45f, 0.45f, 0.7f);

    [Header("Receipt layout")]
    [SerializeField] private Vector2 receiptItemIconPosition = new(22f, -5f);
    [SerializeField] private Vector2 receiptItemIconSize = new(56f, 56f);
    [SerializeField] private Vector2 receiptItemTextPosition = new(8f, -64f);
    [SerializeField] private Vector2 receiptItemTextSize = new(84f, 34f);
    [SerializeField] private Vector2 receiptItemPricePosition = new(100f, -34f);
    [SerializeField] private Vector2 receiptItemPriceSize = new(52f, 32f);
    [SerializeField] private Vector2 receiptSummaryTextPosition = new(8f, -4f);
    [SerializeField] private Vector2 receiptSummaryTextSize = new(76f, 52f);
    [SerializeField] private Vector2 receiptSummaryPricePosition = new(88f, -4f);
    [SerializeField] private Vector2 receiptSummaryPriceSize = new(60f, 52f);
    [SerializeField] private float receiptItemRowHeight = 100f;
    [SerializeField] private float receiptSummaryRowHeight = 60f;
    [SerializeField, Min(1f)] private float receiptFontSizeMin = 14f;
    [SerializeField, Min(1f)] private float receiptFontSizeMax = 18f;

    private Color defaultTextColor;
    private bool initializedColor;

    private void Awake()
    {
        if (background == null) background = GetComponent<Image>();
        if (specsText != null && !initializedColor)
        {
            defaultTextColor = specsText.color;
            initializedColor = true;
        }
    }

    public void Setup(Recipe recipe, bool isCompleted = false)
    {
        if (receiptPriceText != null)
            receiptPriceText.gameObject.SetActive(false);

        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.minHeight = -1f;
            layoutElement.preferredHeight = -1f;
        }

        if (specsText != null && !initializedColor)
        {
            defaultTextColor = specsText.color;
            initializedColor = true;
        }

        if (recipe == null)
        {
            ApplyBackground(0);
            ApplyIcon(null);
            ApplySpecs(null, 0, false);
            return;
        }

        var validSpecs = recipe.ticketSpecs?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        int count = validSpecs != null ? validSpecs.Length : 0;

        ApplyBackground(count);
        ApplyIcon(recipe.ticketIcon);
        ApplySpecs(validSpecs, count, isCompleted);
    }

    public void SetupReceiptLine(Sprite itemIcon, string label, float price, bool emphasize = false)
    {
        if (specsText != null && !initializedColor)
        {
            defaultTextColor = specsText.color;
            initializedColor = true;
        }

        ApplyBackground(itemIcon != null ? 1 : 0);
        ApplyIcon(itemIcon);

        if (specsText == null || receiptPriceText == null)
        {
            Debug.LogError("TicketLineView needs label and price text fields to display a receipt.");
            return;
        }

        bool hasIcon = itemIcon != null;
        if (hasIcon && icon != null)
        {
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchoredPosition = receiptItemIconPosition;
            iconRect.sizeDelta = receiptItemIconSize;
        }

        RectTransform rowRect = transform as RectTransform;
        float rowHeight = hasIcon ? receiptItemRowHeight : receiptSummaryRowHeight;
        if (rowRect != null)
        {
            Vector2 rowSize = rowRect.sizeDelta;
            rowSize.y = rowHeight;
            rowRect.sizeDelta = rowSize;
        }

        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.minHeight = rowHeight;
            layoutElement.preferredHeight = rowHeight;
        }

        string amount = "$" + price.ToString("0.00", CultureInfo.InvariantCulture);
        ConfigureReceiptText(
            specsText,
            hasIcon ? receiptItemTextPosition : receiptSummaryTextPosition,
            hasIcon ? receiptItemTextSize : receiptSummaryTextSize,
            hasIcon ? TextAlignmentOptions.Center : TextAlignmentOptions.Left);
        ConfigureReceiptText(
            receiptPriceText,
            hasIcon ? receiptItemPricePosition : receiptSummaryPricePosition,
            hasIcon ? receiptItemPriceSize : receiptSummaryPriceSize,
            TextAlignmentOptions.Right);

        specsText.text = emphasize ? $"<b>{label}</b>" : label;
        receiptPriceText.text = emphasize ? $"<b>{amount}</b>" : amount;
        specsText.color = defaultTextColor;
        receiptPriceText.color = defaultTextColor;
    }

    private void ConfigureReceiptText(TMP_Text text, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
    {
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(0f, 1f);
        textRect.pivot = new Vector2(0f, 1f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;

        text.fontSize = receiptFontSizeMax;
        text.fontSizeMin = receiptFontSizeMin;
        text.fontSizeMax = receiptFontSizeMax;
        text.enableAutoSizing = true;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate;
        text.alignment = alignment;
        text.gameObject.SetActive(true);
    }

    private void ApplyBackground(int specCount)
    {
        if (background == null || backgrounds == null || backgrounds.Length == 0) return;

        int desiredIndex = specCount <= 0 ? 0 : (specCount == 1 ? 1 : 2);
        int index = Mathf.Clamp(desiredIndex, 0, backgrounds.Length - 1);
        Sprite chosen = backgrounds[index] != null ? backgrounds[index] : backgrounds[0];

        if (chosen != null)
        {
            background.enabled = true;
            background.sprite = chosen;
            background.SetNativeSize();
        }
    }

    private void ApplyIcon(Sprite sprite)
    {
        if (icon == null) return;
        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }

    private void ApplySpecs(string[] validSpecs, int count, bool isCompleted)
    {
        if (specsText == null) return;

        if (count > 0 && validSpecs != null)
        {
            specsText.gameObject.SetActive(true);
            string joined = string.Join("\n", validSpecs);
            specsText.text = isCompleted ? $"<s>{joined}</s>" : joined;
            specsText.color = isCompleted ? completedTextColor : defaultTextColor;
        }
        else if (isCompleted)
        {
            // Even if there were no specs, show a strikethrough marker if completed
            specsText.gameObject.SetActive(true);
            specsText.text = "<s>Done</s>";
            specsText.color = completedTextColor;
        }
        else
        {
            specsText.text = string.Empty;
            specsText.gameObject.SetActive(false);
            specsText.color = defaultTextColor;
        }
    }
}