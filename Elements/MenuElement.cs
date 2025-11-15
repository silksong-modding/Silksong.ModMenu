using System;
using System.Collections.Generic;
using Silksong.ModMenu.Internal;
using UnityEngine;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Abstraction for all objects that can occupy space in a layout.
/// </summary>
public abstract class MenuElement : MenuDisposable, IMenuEntity
{
    private readonly VisibilityManager visibility = new();

    protected MenuElement(GameObject container)
    {
        Container = container;
        RectTransform = container.GetComponent<RectTransform>();

        visibility.OnVisibilityChanged += container.SetActive;
        container.SetActive(true);
        container.AddComponent<OnDestroyHelper>().Action += Dispose;
        OnStateChanged += _ => MaybeApplyDefaultColors();
    }

    /// <inheritdoc/>
    public VisibilityManager Visibility => visibility;

    /// <summary>
    /// The actual GameObject containing all pieces of this MenuElement.
    /// </summary>
    public readonly GameObject Container;

    /// <summary>
    /// The RectTransform for this UI element.
    /// </summary>
    public readonly RectTransform RectTransform;

    /// <summary>
    /// A general descriptor of the state of this MenuElement.
    /// This has no functional purpose other than to make applying standard Color coding easier.
    /// </summary>
    public ElementState State
    {
        get => field;
        set
        {
            if (field == value)
                return;

            field = value;
            OnStateChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Event notified whenever the ElementState changes.
    /// </summary>
    public event Action<ElementState>? OnStateChanged;

    /// <summary>
    /// Convenience accessor for visibility changes.
    /// </summary>
    public event Action<bool> OnVisibilityChanged
    {
        add => visibility.OnVisibilityChanged += value;
        remove => visibility.OnVisibilityChanged -= value;
    }

    /// <inheritdoc/>
    public IEnumerable<MenuElement> AllElements() => [this];

    /// <summary>
    /// Set the primary color of text and other assets within this MenuElement.
    /// </summary>
    public virtual void SetMainColor(Color color) { }

    /// <summary>
    /// If true, apply standard colors from `Colors` on any state changes.
    /// </summary>
    public bool ApplyDefaultColors = true;

    protected void MaybeApplyDefaultColors()
    {
        if (!ApplyDefaultColors)
            return;
        ApplyDefaultColorsImpl();
    }

    /// <inheritdoc/>
    public void UpdateLayout(Vector2 localAnchorPos) =>
        RectTransform.SetAnchoredPosition(localAnchorPos);

    protected virtual void ApplyDefaultColorsImpl() => Colors.GetDefaultColor(this);

    /// <summary>
    /// Set the general font size of the text(s) within the MenuElement.
    /// </summary>
    public virtual void SetFontSizes(FontSizes fontSizes) { }

    /// <inheritdoc/>
    public void SetMenuParent(IMenuEntity parent) => visibility.SetParent(parent.Visibility);

    /// <summary>
    /// Add this MenuElement directly to a UI GameObject without an associated IMenuEntity.
    /// </summary>
    public void SetGameObjectParent(GameObject container)
    {
        if (Container.transform.parent != null)
            throw new ArgumentException("GameObject parent already set");

        Container.transform.SetParent(container.transform, false);
    }
}
