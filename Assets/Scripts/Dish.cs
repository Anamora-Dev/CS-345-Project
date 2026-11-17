using UnityEngine;

/// One script for every dish. Put it on a single "Dish" prefab with two child sprites:
///   PlateSprite (the plate design, picked once when the plate is spawned)
///   FoodSprite  (changes as the dish gets built: toast -> avocado -> egg)
public class Dish : MonoBehaviour
{
    public enum Stage { Empty, Toast, Avocado, AvocadoEgg }

    [Header("Renderers")]
    [SerializeField] SpriteRenderer plateRenderer;
    [SerializeField] SpriteRenderer foodRenderer;

    [Header("Food sprite per stage (Empty is ignored)")]
    [SerializeField] Sprite toastSprite;
    [SerializeField] Sprite avocadoToastSprite;
    [SerializeField] Sprite avocadoEggSprite;

    public Stage CurrentStage { get; private set; } = Stage.Empty;

    void Awake() => SetStage(CurrentStage);

    /// Called once by the plate stack. The plate never changes after this.
    public void SetPlate(Sprite design, Color tint, bool flipX)
    {
        if (design != null) plateRenderer.sprite = design;
        plateRenderer.color = tint;
        plateRenderer.flipX = flipX;
    }

    /// Call this from your cooking logic, e.g. dish.SetStage(Dish.Stage.Avocado);
    public void SetStage(Stage stage)
    {
        CurrentStage = stage;

        switch (stage)
        {
            case Stage.Toast:      foodRenderer.sprite = toastSprite; break;
            case Stage.Avocado:    foodRenderer.sprite = avocadoToastSprite; break;
            case Stage.AvocadoEgg: foodRenderer.sprite = avocadoEggSprite; break;
        }
        foodRenderer.enabled = stage != Stage.Empty;
    }

    public void SetSortingOrder(int order)
    {
        plateRenderer.sortingOrder = order;
        foodRenderer.sortingOrder = order + 1; // food always draws on top of its plate
    }
}
