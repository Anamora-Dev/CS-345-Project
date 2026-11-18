
using System.Collections.Generic;
using UnityEngine;

public class RecipeTracker
{
    class Candidate
    {
        public Recipe recipe;
        public int progress;

        public Candidate(Recipe recipe)
        {
            this.recipe = recipe;
            progress = 0;
        }
    }

    readonly Recipe[] recipes;
    readonly List<Candidate> candidates = new();

    public RecipeTracker(Recipe[] recipes)
    {
        this.recipes = recipes;
        Reset();
    }

    public int Count =>
        candidates.Count > 0 ? candidates[0].progress : 0;

    void Reset()
    {
        candidates.Clear();

        if (recipes == null) return;

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null || recipe.steps == null ||
                recipe.steps.Length == 0)
                continue;

            candidates.Add(new Candidate(recipe));
        }
    }

    public bool TryAdd(
        IngredientData next,
        out Recipe matched,
        out int stepIndex)
    {
        matched = null;
        stepIndex = Count;

        if (next == null || candidates.Count == 0)
            return false;

        List<Candidate> matching = new();

        foreach (Candidate candidate in candidates)
        {
            if (candidate.progress >= candidate.recipe.steps.Length)
                continue;

            if (candidate.recipe.steps[candidate.progress].ingredient != next)
                continue;

            matching.Add(new Candidate(candidate.recipe)
            {
                progress = candidate.progress + 1
            });
        }

        if (matching.Count == 0)
        {
            Debug.Log($"[RecipeTracker] Rejected ingredient: {next.name}");
            return false;
        }

        candidates.Clear();
        candidates.AddRange(matching);

        matched = candidates[0].recipe;
        return true;
    }

    public bool IsComplete(Recipe recipe)
    {
        if (recipe == null) return false;

        bool complete = false;
        bool longerPossible = false;

        foreach (Candidate candidate in candidates)
        {
            if (candidate.recipe == recipe &&
                candidate.progress == recipe.steps.Length)
            {
                complete = true;
            }

            if (candidate.progress < candidate.recipe.steps.Length)
                longerPossible = true;
        }

        return complete && !longerPossible;
    }

    public void Clear()
    {
        Reset();
    }
}
