using System;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A wrapped value that notifies subscribers on change.
/// </summary>
public interface IValueModel<T> : IBaseValueModel
{
    /// <summary>
    /// Event notified whenever the modeled value is changed.
    /// </summary>
    public event Action<T>? OnValueChanged;

    /// <summary>
    /// Returns the currently selected value.
    /// </summary>
    T GetValue();

    object IBaseValueModel.GetRawValue() => GetValue()!;

    /// <summary>
    /// Explicitly sets the value selected by this model.
    /// </summary>
    /// <param name="value">The newly selected value.</param>
    /// <returns>True if the value was accepted, false if rejected.</returns>
    bool SetValue(T value);

    bool IBaseValueModel.SetRawValue(object value) => value is T item && SetValue(item);

    /// <summary>
    /// Convience accessor for the value, throwing on invalid writes.
    /// </summary>
    T Value { get; set; }
}
