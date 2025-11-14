using System;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A MenuElement that is Selectable, and should be included in layout-based navigation.
/// </summary>
public abstract class SelectableElement : MenuElement
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

    #region Navigation
    // Helpers for setting explicit navigation between elements.
    public void SetNavUp(Selectable selectable) => SelectableComponent.SetNavUp(selectable);

    public void SetNavUp(SelectableElement selectableElement) =>
        SelectableComponent.SetNavUp(selectableElement.SelectableComponent);

    public void ClearNavUp() => SelectableComponent.ClearNavUp();

    public void SetNavLeft(Selectable selectable) => SelectableComponent.SetNavLeft(selectable);

    public void SetNavLeft(SelectableElement selectableElement) =>
        SelectableComponent.SetNavLeft(selectableElement.SelectableComponent);

    public void ClearNavLeft() => SelectableComponent.ClearNavLeft();

    public void SetNavRight(Selectable selectable) => SelectableComponent.SetNavRight(selectable);

    public void SetNavRight(SelectableElement selectableElement) =>
        SelectableComponent.SetNavRight(selectableElement.SelectableComponent);

    public void ClearNavRight() => SelectableComponent.ClearNavRight();

    public void SetNavDown(Selectable selectable) => SelectableComponent.SetNavDown(selectable);

    public void SetNavDown(SelectableElement selectableElement) =>
        SelectableComponent.SetNavDown(selectableElement.SelectableComponent);

    public void ClearNavDown() => SelectableComponent.ClearNavDown();

    public void ClearNav() => SelectableComponent.ClearNav();
    #endregion

    protected override void ApplyDefaultColorsImpl() => SetMainColor(Colors.GetDefaultColor(this));
}
