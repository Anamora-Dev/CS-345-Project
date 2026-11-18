using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlateSpawner : DishSpawner<Plate, PlateDesign>
{
    protected override void InitializeDish(Plate plate, PlateDesign design)
    {
        plate.Init(design);
    }
}