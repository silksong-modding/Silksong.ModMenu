using System;
using Silksong.ModMenu.Internal;
using UnityEngine;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Abstraction for all objects that can occupy space in a layout.
/// </summary>
public abstract class MenuElement : MenuDisposable
{
    protected MenuElement(GameObject container)
    {
        Container = container;
        RectTransform = container.GetComponent<RectTransform>();

        container.AddComponent<OnDestroyHelper>().Action += Dispose;
        OnStateChanged += _ => MaybeApplyDefaultColors();
    }

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
    /// Convenience accessor for whether this MenuElement should be visible or not.
    /// Listeners are not notified if this is changed directly, rather than through the accessor.
    ///
    /// Layouts may choose to listen for visibility changes in order to reflow surrounding elements.
    /// </summary>
    public bool Visible
    {
        get => Container.activeSelf;
        set
        {
            if (Visible == value)
                return;

            Container.SetActive(value);
            OnVisibilityChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Event notified whenever the intended visibility of this element changes.
    /// </summary>
    public event Action<bool>? OnVisibilityChanged;

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

    protected virtual void ApplyDefaultColorsImpl() => Colors.GetDefaultColor(this);

    /// <summary>
    /// Set the general font size of the text(s) within the MenuElement.
    /// </summary>
    public virtual void SetFontSizes(FontSizes fontSizes) { }

    // Whether this element has been added to a layout yet.
    private bool Headless = true;

    internal void AddToContainer(GameObject container)
    {
        if (!Headless)
            throw new InvalidOperationException("Element already added to container.");

        Headless = false;
        Container.transform.SetParent(container.transform, false);
        Container.SetActive(true);
    }
}
