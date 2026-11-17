using UnityEngine;

[CreateAssetMenu(menuName = "Food/Recipe")]
public class Recipe : ScriptableObject
{
    [System.Serializable]
    public struct Step
    {
        public IngredientData ingredient;
        public Sprite plateSprite;
        public GameObject plateVisual;   // optional: used instead of plateSprite if set
    }

    public Step[] steps;
}