using UnityEngine;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A scrolling panel containing an arbitrary collection of menu elements where each
/// has a manually assigned position.
/// Navigation to defaults to the single left/right/top/bottom-most element within.
/// </summary>
public class ScrollingFreeGroup() : AbstractScrollingGroup<FreeGroup>(new())
{
    /// <inheritdoc cref="FreeGroup.Count"/>
    public int Count => InternalGroup.Count;

    /// <inheritdoc cref="FreeGroup.TryGetOffset"/>
    public bool TryGetOffset(MenuElement element, out Vector2 offset) =>
        InternalGroup.TryGetOffset(element, out offset);

    /// <inheritdoc cref="FreeGroup.Add"/>
    public void Add(MenuElement element, Vector2 offset)
    {
        InternalGroup.Add(element, offset);
        AddChild(element);
    }

    /// <inheritdoc cref="FreeGroup.Update"/>
    public void Update(MenuElement element, Vector2 offset)
    {
        InternalGroup.Update(element, offset);
        RecalculateContentPaneSize();
    }

    /// <inheritdoc cref="FreeGroup.Remove"/>
    public bool Remove(MenuElement element)
    {
        bool result = InternalGroup.Remove(element);
        if (result)
            RecalculateContentPaneSize();
        return result;
    }
}
