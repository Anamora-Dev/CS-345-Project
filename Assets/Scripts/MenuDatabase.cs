using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Food/Menu Database")]
public class MenuDatabase : ScriptableObject
{
    [Serializable]
    public struct RecipeWeight
    {
        public Recipe recipe;
        [Min(0.01f)] public float weight;
    }

    [Serializable]
    public class LocationProfile
    {
        public string locationName = "Location 1";
        public List<RecipeWeight> weights = new();

        public float GetWeight(Recipe recipe)
        {
            if (recipe == null || weights == null) return 1f;
            foreach (var entry in weights)
            {
                if (entry.recipe == recipe)
                    return Mathf.Max(0.01f, entry.weight);
            }
            return 1f;
        }
    }

    [Header("Recipes")]
    [Tooltip("All recipes in the game.")]
    public List<Recipe> allRecipes = new();

    [Tooltip("Recipes available from the start (can also be unlocked at runtime).")]
    public List<Recipe> unlockedRecipes = new();

    [Header("Locations")]
    public List<LocationProfile> locations = new();

    public bool IsUnlocked(Recipe recipe) =>
        recipe != null && unlockedRecipes.Contains(recipe);

    public void UnlockRecipe(Recipe recipe)
    {
        if (recipe != null && !unlockedRecipes.Contains(recipe))
            unlockedRecipes.Add(recipe);
    }

    public Recipe PickWeightedRecipe(int locationIndex)
    {
        if (unlockedRecipes == null || unlockedRecipes.Count == 0) return null;

        LocationProfile profile = (locations != null && locationIndex >= 0 && locationIndex < locations.Count)
            ? locations[locationIndex]
            : null;

        float totalWeight = 0f;
        foreach (var r in unlockedRecipes)
        {
            if (r == null) continue;
            totalWeight += profile != null ? profile.GetWeight(r) : 1f;
        }

        if (totalWeight <= 0f) return unlockedRecipes[0];

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var r in unlockedRecipes)
        {
            if (r == null) continue;
            cumulative += profile != null ? profile.GetWeight(r) : 1f;
            if (roll <= cumulative)
                return r;
        }

        return unlockedRecipes[unlockedRecipes.Count - 1];
    }
}