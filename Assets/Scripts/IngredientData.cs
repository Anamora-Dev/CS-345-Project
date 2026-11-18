
using System;
using UnityEngine;

public enum ProcessAction { None, Unwrap, Chop, Bake }

[CreateAssetMenu(menuName = "Food/Ingredient")]
public class IngredientData : ScriptableObject
{
    public Sprite dragSprite;

    [Serializable]
    public struct Transformation
    {
        public ProcessAction action;
        public Ingredient result;
    }

    public Transformation[] transformations;

    public bool HasAnyTransformation
    {
        get
        {
            if (transformations == null) return false;
            foreach (var t in transformations)
                if (t.result != null) return true;
            return false;
        }
    }

    public Ingredient ResultFor(ProcessAction action)
    {
        if (transformations == null) return null;
        foreach (var t in transformations)
            if (t.action == action) return t.result;
        return null;
    }
}
