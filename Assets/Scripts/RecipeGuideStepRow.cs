using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeGuideStepRow : MonoBehaviour
{
    [SerializeField] private TMP_Text stepLabel;
    [SerializeField] private RectTransform iconsContainer;
    [SerializeField] private Image iconPrefab;

    private readonly List<GameObject> spawned = new();

    public void Setup(int stepNumber, string tool, Sprite[] icons)
    {
        stepLabel.text = string.IsNullOrWhiteSpace(tool)
            ? $"Step {stepNumber}: Assembly"
            : $"Step {stepNumber}: {tool}";

        foreach (var go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();

        if (icons == null) return;

        foreach (var sprite in icons)
        {
            if (sprite == null) continue;

            Image img = Instantiate(iconPrefab, iconsContainer, false);
            img.gameObject.SetActive(true);
            img.sprite = sprite;
            img.preserveAspect = true;   // keeps each sprite's true proportions
            img.raycastTarget = false;
            spawned.Add(img.gameObject);
        }
    }
}