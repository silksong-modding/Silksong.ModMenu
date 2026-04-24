using System.Collections.Generic;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A scrolling panel containing a single column of menu elements, evenly spaced.
/// </summary>
public class ScrollingVerticalGroup() : AbstractScrollingGroup<VerticalGroup>(new())
{
    /// <inheritdoc cref="VerticalGroup.VerticalSpacing"/>
    public float VerticalSpacing
    {
        get => InternalGroup.VerticalSpacing;
        set
        {
            if (InternalGroup.VerticalSpacing != value)
            {
                InternalGroup.VerticalSpacing = value;
                RecalculateContentPaneSize();
            }
        }
    }

    /// <inheritdoc cref="VerticalGroup.Add"/>
    public void Add(MenuElement element)
    {
        InternalGroup.Add(element);
        AddChild(element);
    }

    /// <inheritdoc cref="VerticalGroup.AddRange"/>
    public void AddRange(IEnumerable<MenuElement> elements)
    {
        foreach (var element in elements)
            Add(element);
    }

    /// <inheritdoc cref="VerticalGroup.Insert"/>
    public void Insert(int index, MenuElement element)
    {
        InternalGroup.Insert(index, element);
        AddChild(element);
    }

    /// <inheritdoc cref="VerticalGroup.Remove"/>
    public bool Remove(MenuElement element)
    {
        bool result = InternalGroup.Remove(element);
        if (result)
            RecalculateContentPaneSize();
        return result;
    }

    /// <inheritdoc cref="VerticalGroup.RemoveAt"/>
    public void RemoveAt(int index)
    {
        InternalGroup.RemoveAt(index);
        RecalculateContentPaneSize();
    }
}
