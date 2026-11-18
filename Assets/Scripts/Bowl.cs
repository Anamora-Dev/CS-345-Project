public class Bowl : Dish
{
    // Assumes BowlDesign has a `sprite` field like PlateDesign does.
    public void Init(BowlDesign design) => SetBaseSprite(design.sprite);
}
