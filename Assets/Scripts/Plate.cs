
using UnityEngine;

public class Plate : Dish
{
    public void Init(PlateDesign design)
    {
        SetBaseSprite(design.sprite);
    }
}
