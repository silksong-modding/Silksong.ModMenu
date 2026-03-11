using System;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Models;

/// <summary>
/// Base class which implements the main value model interfaces for a specific type.
/// Subclasses need to only provide the getter and setter.
/// </summary>
public abstract class AbstractValueModel<T> : IValueModel<T>
{
    /// <summary>
    /// Construct an AbstractValueModel with the default value.
    /// </summary>
    protected AbstractValueModel() => OnValueChanged += o => OnObjectValueChangedImpl?.Invoke(o!);

    /// <summary>
    /// Construct an AbstractValueModel with the specified value.
    /// </summary>
    protected AbstractValueModel(T value)
        : this()
    {
        Value = value;
    }

    /// <inheritdoc/>
    public event Action<T>? OnValueChanged;

    /// <summary>
    /// Subclasses should call this if they modify the stored value without calling 'SetValue()'.
    /// </summary>
    protected void InvokeOnValueChanged() => OnValueChanged?.Invoke(GetValue());

    private event Action<object>? OnObjectValueChangedImpl;

    /// <inheritdoc/>
    event Action<object>? IBaseValueModel.OnRawValueChanged
    {
        add => OnObjectValueChangedImpl += value;
        remove => OnObjectValueChangedImpl -= value;
    }

    /// <inheritdoc/>
    public abstract T GetValue();

    /// <inheritdoc/>
    public abstract bool SetValue(T value);

    /// <inheritdoc/>
    public T Value
    {
        get => GetValue();
        set
        {
            if (!SetValue(value))
                throw new ArgumentException($"Invalid value: {value}");
        }
    }

    /// <inheritdoc/>
    public virtual LocalizedText DisplayString() => $"{GetValue()}";
}
