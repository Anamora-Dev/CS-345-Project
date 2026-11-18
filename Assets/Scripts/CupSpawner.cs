using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CupSpawner : DishSpawner<Cup, CupDesign>
{
    protected override void InitializeDish(Cup cup, CupDesign design)
    {
        cup.Init(design);
    }
}