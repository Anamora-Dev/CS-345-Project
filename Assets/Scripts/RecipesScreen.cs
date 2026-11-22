using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipesApp : MonoBehaviour
{
    [Header("Views")]
    public GameObject listView;
    public GameObject detailView;

    [Header("List")]
    public Transform listContent;
    public RecipeRow rowPrefab;

    [Header("Detail")]
    public TMP_Text titleText;
    public Transform detailContent;
    public ScrollRect detailScroll;
    public RecipeGuideStepRow guideRowPrefab;

    void OnEnable()
    {
        RecipeBook.Instance.OnChanged += BuildList;
        ShowList();
    }

    void OnDisable()
    {
        if (RecipeBook.Instance != null)
            RecipeBook.Instance.OnChanged -= BuildList;
    }

    public void ShowList()
    {
        detailView.SetActive(false);
        listView.SetActive(true);
        BuildList();
    }

    void BuildList()
    {
        Clear(listContent);
        foreach (var r in RecipeBook.Instance.allRecipes)
        {
            var row = Instantiate(rowPrefab, listContent);
            row.Init(r, RecipeBook.Instance.IsUnlocked(r), ShowDetail);
        }
    }

    void ShowDetail(Recipe recipe)
    {
        listView.SetActive(false);
        detailView.SetActive(true);
        titleText.text = recipe.displayName;
        Clear(detailContent);

        int stepNum = 1;

        if (recipe.guideLines != null)
            foreach (var line in recipe.guideLines)
                AddRow(stepNum++, line.tool, line.icons);

        if (recipe.showAssemblyRow)
            AddRow(stepNum++, "Assemble", GetAssemblyStack(recipe).ToArray());

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)detailContent);
        if (detailScroll != null) detailScroll.verticalNormalizedPosition = 1f; // scroll to top
    }

    void AddRow(int stepNumber, string tool, Sprite[] sprites)
    {
        var row = Instantiate(guideRowPrefab, detailContent);
        row.Setup(stepNumber, tool, sprites);
    }

    List<Sprite> GetAssemblyStack(Recipe recipe)
    {
        var stack = new List<(int step, Sprite sprite)>();

        for (int i = 0; i < recipe.steps.Length; i++)
        {
            var step = recipe.steps[i];
            Sprite s = step.plateSprite;
            if (s == null && step.plateVisual != null)
            {
                var sr = step.plateVisual.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) s = sr.sprite;
            }
            if (s == null) continue;

            switch (step.visualAction)
            {
                case Recipe.VisualAction.AddLayer:
                    stack.Add((i, s));
                    break;
                case Recipe.VisualAction.ReplaceWholePlate:
                    stack.Clear();
                    stack.Add((i, s));
                    break;
                case Recipe.VisualAction.ReplacePreviousLayer:
                    int idx = stack.FindIndex(e => e.step == step.targetStepIndex);
                    if (idx >= 0) stack[idx] = (i, s);
                    else stack.Add((i, s));
                    break;
            }
        }

        var result = new List<Sprite>();
        foreach (var e in stack) result.Add(e.sprite);
        return result;
    }

    void Clear(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            child.SetActive(false);   // layout ignores it immediately
            Destroy(child);
        }
    }
}