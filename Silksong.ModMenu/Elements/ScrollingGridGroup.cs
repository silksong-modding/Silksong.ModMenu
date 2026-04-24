namespace Silksong.ModMenu.Elements;

/// <summary>
/// A scrolling panel containing a fixed-width grid with a specific
/// number of columns and an unbounded number of rows.
/// </summary>
public class ScrollingGridGroup(int columns) : AbstractScrollingGroup<GridGroup>(new(columns))
{
    /// <inheritdoc cref="GridGroup.Columns"/>
    public int Columns => InternalGroup.Columns;

    /// <inheritdoc cref="GridGroup.Rows"/>
    public int Rows => InternalGroup.Rows;

    /// <inheritdoc cref="GridGroup.HorizontalSpacing"/>
    public float HorizontalSpacing
    {
        get => InternalGroup.HorizontalSpacing;
        set
        {
            if (InternalGroup.HorizontalSpacing != value)
            {
                InternalGroup.HorizontalSpacing = value;
                RecalculateContentPaneSize();
            }
        }
    }

    /// <inheritdoc cref="GridGroup.VerticalSpacing"/>
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

    /// <inheritdoc cref="GridGroup.Add"/>
    public void Add(MenuElement element)
    {
        InternalGroup.Add(element);
        AddChild(element);
    }

    /// <inheritdoc cref="GridGroup.AddAt"/>
    public void AddAt(int row, int column, MenuElement element)
    {
        InternalGroup.AddAt(row, column, element);
        AddChild(element);
    }

    /// <inheritdoc cref="GridGroup.Remove"/>
    public bool Remove(MenuElement element)
    {
        bool result = InternalGroup.Remove(element);
        if (result)
            RecalculateContentPaneSize();
        return result;
    }

    /// <inheritdoc cref="GridGroup.RemoveAt"/>
    public void RemoveAt(int row, int column)
    {
        InternalGroup.RemoveAt(row, column);
        RecalculateContentPaneSize();
    }
}
