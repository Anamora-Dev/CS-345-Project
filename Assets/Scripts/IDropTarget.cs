/// Anything an ingredient can be dropped onto: plates, vessels (pot/pan), stations (cutting board), trash...
/// Return true if the item was used. The Ingredient destroys itself when that happens.
public interface IDropTarget
{
    bool TryAccept(Ingredient item);
}
