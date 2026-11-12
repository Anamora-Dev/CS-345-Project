
using UnityEngine;

public class BreadItem : MonoBehaviour
{
    public enum BreadState
    {
        Raw,
        Toasting,
        Toasted,
        Plated
    }

    public BreadState state = BreadState.Raw;

    public Sprite rawSprite;
    public Sprite toastedSprite;

    public SpriteRenderer spriteRenderer;
    public Draggable draggable;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (draggable == null)
            draggable = GetComponent<Draggable>();
    }

    public void SetState(BreadState newState)
    {
        state = newState;

        if (state == BreadState.Raw)
            spriteRenderer.sprite = rawSprite;

        if (state == BreadState.Toasted)
            spriteRenderer.sprite = toastedSprite;

        if (draggable != null)
            draggable.canDrag = state == BreadState.Raw ||
                                state == BreadState.Toasted;
    }
}
