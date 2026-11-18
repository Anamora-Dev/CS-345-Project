public class Cup : Dish
{
    // Assumes MugDesign has a `sprite` field like PlateDesign does.
    public void Init(CupDesign design) => SetBaseSprite(design.sprite);
}
