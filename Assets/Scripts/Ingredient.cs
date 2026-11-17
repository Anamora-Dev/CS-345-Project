using UnityEngine;

[RequireComponent(typeof(Draggable))]
public class Ingredient : MonoBehaviour
{
    public IngredientData data;

    Draggable drag;

    void Awake() => drag = GetComponent<Draggable>();
    void OnEnable() => drag.Dropped += OnDropped;
    void OnDisable() => drag.Dropped -= OnDropped;

    void OnDropped(Draggable d)
    {
        var hits = Physics2D.OverlapPointAll(transform.position);
        Debug.Log($"[Ingredient] {name} dropped, data = {(data ? data.name : "NULL")}, colliders under it: {hits.Length}");

        foreach (var hit in hits)
        {
            Debug.Log($"[Ingredient] hit {hit.name}");
            if (hit.transform.IsChildOf(transform)) continue;

            var target = hit.GetComponentInParent<IDropTarget>();
            if (target == null) { Debug.Log($"[Ingredient] {hit.name} is not a drop target"); continue; }

            if (target.TryAccept(data))
            {
                Destroy(gameObject);
                return;
            }
            Debug.Log("[Ingredient] plate rejected it (wrong ingredient/order, or no matching recipe)");
        }
        transform.position = drag.HomePosition;
    }
}