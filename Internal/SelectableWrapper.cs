using System;
using System.Diagnostics.CodeAnalysis;
using Silksong.ModMenu.Elements;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal class SelectableWrapper(Selectable selectable) : INavigable
{
    private readonly Selectable selectable = selectable;

    private Navigation Nav
    {
        get => selectable.navigation;
        set =>
            selectable.navigation = value with
            {
                mode = Navigation.Mode.Explicit,
                wrapAround = false,
            };
    }

    public void ClearNeighbor(NavigationDirection direction) =>
        Nav = direction switch
        {
            NavigationDirection.Up => Nav with { selectOnUp = null },
            NavigationDirection.Left => Nav with { selectOnLeft = null },
            NavigationDirection.Right => Nav with { selectOnRight = null },
            NavigationDirection.Down => Nav with { selectOnDown = null },
            _ => throw direction.UnsupportedEnum(),
        };

    public void ClearNeighbors() => Nav = new();

    public bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    )
    {
        selectable = this.selectable;
        return true;
    }

    public void SetNeighbor(NavigationDirection direction, Selectable selectable) =>
        Nav = direction switch
        {
            NavigationDirection.Up => Nav with { selectOnUp = selectable },
            NavigationDirection.Left => Nav with { selectOnLeft = selectable },
            NavigationDirection.Right => Nav with { selectOnRight = selectable },
            NavigationDirection.Down => Nav with { selectOnDown = selectable },
            _ => throw direction.UnsupportedEnum(),
        };
}
