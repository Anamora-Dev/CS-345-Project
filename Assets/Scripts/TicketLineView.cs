using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketLineView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text specsText;

    [Header("Backgrounds, shortest to longest")]
    [Tooltip("Element 0 = 0 specs (shortest), element 1 = 1 spec, element 2 = 2+ specs")]
    [SerializeField] private Sprite[] backgrounds;

    [SerializeField] private Image background;
    [SerializeField] private Color completedTextColor = new(0.45f, 0.45f, 0.45f, 0.7f);

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