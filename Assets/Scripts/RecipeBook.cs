using System;
using System.Collections.Generic;
using UnityEngine;

public class RecipeBook : MonoBehaviour
{
    public static RecipeBook Instance { get; private set; }

    public Recipe[] allRecipes;        // every recipe, in the order you want listed
    public Recipe[] startUnlocked;     // the ones available at the start

    HashSet<Recipe> unlocked = new HashSet<Recipe>();
    public event Action OnChanged;

    void Awake()
    {
        Instance = this;
        foreach (var r in startUnlocked) unlocked.Add(r);
    }

    public bool IsUnlocked(Recipe r) => unlocked.Contains(r);

    public void Unlock(Recipe r)
    {
        if (unlocked.Add(r)) OnChanged?.Invoke();
    }
}