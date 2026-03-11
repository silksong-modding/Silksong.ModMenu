using System;
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
    /// <summary>
    /// Construct a basic menu screen with a single content entity.
    /// </summary>
    public BasicMenuScreen(string title, INavigableMenuEntity content)
        : base(title) => Content = content;

    /// <summary>
    /// The content displayed by this menu screen, minus the back button.
    /// </summary>
    public INavigableMenuEntity Content
    {
        get => field;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Content));

            if (field != value)
                field?.ClearParents();

            field = value;
            AddChild(field);
        }
    }

    /// <summary>
    /// Remove the content pane for this menu, showing nothing instead.
    /// </summary>
    public void ClearContent() => Content = new VerticalGroup();

    /// <summary>
    /// Top anchor point for the elements.
    /// </summary>
    public Vector2 Anchor = SpacingConstants.TOP_CENTER_ANCHOR;

    /// <inheritdoc/>
    protected override IEnumerable<IMenuEntity> AllEntities() => [Content];

    /// <inheritdoc/>
    protected override SelectableElement? GetDefaultSelectableInternal() =>
        Content.GetDefaultSelectable();

    /// <inheritdoc/>
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
