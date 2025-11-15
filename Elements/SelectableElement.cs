using System;
using System.Diagnostics.CodeAnalysis;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A MenuElement that is Selectable, and should be included in layout-based navigation.
/// </summary>
public abstract class SelectableElement : MenuElement, INavigableMenuEntity
{
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
    public void ClearNeighbor(NavigationDirection direction) =>
        new SelectableWrapper(SelectableComponent).ClearNeighbor(direction);

    /// <inheritdoc/>
    public void SetNeighbor(NavigationDirection direction, Selectable selectable) =>
        new SelectableWrapper(SelectableComponent).SetNeighbor(direction, selectable);

    /// <inheritdoc/>
    public bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    )
    {
        selectable = SelectableComponent;
        return true;
    }

    /// <inheritdoc/>
    protected override void ApplyDefaultColorsImpl() => SetMainColor(Colors.GetDefaultColor(this));
}
