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
        if (group == null) group = GetComponent<CanvasGroup>();
        if (recipe == null) return;

        label.text = recipe.displayName;
        icon.sprite = recipe.listIcon;
        icon.color = unlocked ? Color.white : Color.black;

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            if (onClick != null)
                button.onClick.AddListener(() => onClick(recipe));
        }

        if (group != null)
            group.alpha = unlocked ? 1f : 0.4f;
    }
}