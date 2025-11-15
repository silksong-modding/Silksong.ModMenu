namespace Silksong.ModMenu.Elements;

/// <summary>
/// A member of the menu element hierarchy that is also navigable.
/// </summary>
public interface INavigableMenuEntity : IMenuEntity, INavigable
{
    /// <summary>
    /// Return an arbitrary element to select within this entity.
    /// </summary>
    SelectableElement? GetDefaultSelectable();
}
