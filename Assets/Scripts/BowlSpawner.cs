using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BowlSpawner : DishSpawner<Bowl, BowlDesign>
{
    protected override void InitializeDish(Bowl bowl, BowlDesign design)
    {
        bowl.Init(design);
    }
}