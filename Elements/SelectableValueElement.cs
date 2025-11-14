using System;
using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A SelectableElement with an associated value model.
/// </summary>
public abstract class SelectableValueElement<T>(
    GameObject container,
    Selectable selectable,
    IValueModel<T> model
) : BaseSelectableValueElement(container, selectable)
{
    /// <summary>
    /// The underlying model.
    /// </summary>
    public readonly IValueModel<T> Model = model;

    /// <inheritdoc/>
    public override IBaseValueModel RawModel => Model;

    /// <summary>
    /// Listener for changes in the selected value.
    /// </summary>
    public event Action<T>? OnValueChanged
    {
        add => Model.OnValueChanged += value;
        remove => Model.OnValueChanged -= value;
    }

    /// <summary>
    /// The value chosen by this menu element.
    /// </summary>
    public T Value
    {
        get => Model.Value;
        set => Model.Value = value;
    }
}
