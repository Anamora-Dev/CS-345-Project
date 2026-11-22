using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeRow : MonoBehaviour
{
    public Image icon;
    public TMP_Text label;
    public CanvasGroup group;   // add a CanvasGroup to the prefab root

    public void Init(Recipe recipe, bool unlocked, Action<Recipe> onClick)
    {
        label.text = recipe.displayName;
        icon.sprite = recipe.listIcon;

        var button = GetComponent<Button>();
        button.interactable = unlocked;
        group.alpha = unlocked ? 1f : 0.4f;   // greyed out
        if (!unlocked) icon.color = Color.black;  // optional silhouette

        button.onClick.AddListener(() => onClick(recipe));
    }
}