using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A MenuElement that is Selectable, and should be included in layout-based navigation.
/// </summary>
public abstract class SelectableElement : MenuElement, INavigableMenuEntity
{
    /// <summary>
    /// Construct a SelectableElement with a given container and a Selectable within it.
    /// </summary>
    protected SelectableElement(GameObject container, Selectable selectable)
        : base(container)
    {
        SelectableComponent = selectable;
        OnInteractableChanged += _ => MaybeApplyDefaultColors();
    }

    /// <summary>
    /// The Selectable component of the menu element.
    /// </summary>
    public readonly Selectable SelectableComponent;

    /// <summary>
    /// Returns whether this element is currently selected.
    /// </summary>
    public bool IsSelected => SelectableComponent.IsSelected();

    /// <summary>
    /// Transparently force this element to be selected.
    /// </summary>
    public void ForceSelect()
    {
        if (IsSelected)
            return;

        SelectableComponent.ForceSelect();
    }

    /// <summary>
    /// Convenience accessor for whether this MenuElement should be interactable or not.
    /// </summary>
    public bool Interactable
    {
        get => SelectableComponent.interactable;
        set
        {
            if (Interactable == value)
                return;

            SelectableComponent.interactable = value;
            OnInteractableChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Event notified whenever the lock state of the element changes.
    /// </summary>
    public event Action<bool>? OnInteractableChanged;

    /// <inheritdoc/>
    public SelectableElement? GetDefaultSelectable() => this;

    /// <inheritdoc/>
    public void ClearNeighbors() => new SelectableWrapper(SelectableComponent).ClearNeighbors();

    /// <inheritdoc/>
    public void ClearNeighbors(NavigationDirection direction) =>
        new SelectableWrapper(SelectableComponent).ClearNeighbors(direction);

    /// <inheritdoc/>
    public void SetNeighbors(NavigationDirection direction, IEnumerable<Selectable> selectables) =>
        new SelectableWrapper(SelectableComponent).SetNeighbors(direction, selectables);

    /// <inheritdoc/>
    public bool GetSelectables(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
    )
    {
        selectables = [SelectableComponent];
        return true;
    }

    /// <inheritdoc/>
    protected override Color GetDefaultColorInternal() => Colors.GetDefaultColor(this);
}
