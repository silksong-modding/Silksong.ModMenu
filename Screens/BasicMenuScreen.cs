using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using UnityEngine;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A simple menu screen with a single content entity.
/// </summary>
public class BasicMenuScreen : AbstractMenuScreen
{
    public BasicMenuScreen(string title, INavigableMenuEntity content)
        : base(title)
    {
        Content = content;
        content.SetGameObjectParent(ContentPane);
    }

    /// <summary>
    /// The content displayed by this menu screen, minus the back button.
    /// </summary>
    public readonly INavigableMenuEntity Content;

    /// <summary>
    /// Top anchor point for the elements.
    /// </summary>
    public Vector2 Anchor = SpacingConstants.TOP_CENTER_ANCHOR;

    protected override IEnumerable<MenuElement> AllElements() => Content.AllElements();

    protected override SelectableElement? GetDefaultSelectableInternal() =>
        Content.GetDefaultSelectable();

    protected override void UpdateLayout()
    {
        Content.UpdateLayout(Anchor);

        SelectableWrapper wrapper = new(BackButton);
        wrapper.ClearNeighbors();

        Content.SetNeighborDown(BackButton);
        Content.SetNeighborUp(BackButton);
        if (Content.GetNeighborDown(out var selectable))
            wrapper.SetNeighborDown(selectable);
        if (Content.GetNeighborUp(out selectable))
            wrapper.SetNeighborUp(selectable);
    }
}
