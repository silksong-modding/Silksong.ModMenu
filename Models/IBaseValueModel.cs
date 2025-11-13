using System;

namespace Silksong.ModMenu.Models;

/// <summary>
/// An IValueModel defined with erasure, for use in reflection-based environments.
/// </summary>
public interface IBaseValueModel : IDisplayable
{
    /// <summary>
    /// Event notified whenever the modeled value is changed.
    /// </summary>
    public event Action<object>? OnRawValueChanged;

    /// <summary>
    /// Returns the currently selected value.
    /// </summary>
    object GetRawValue();

    /// <summary>
    /// Explicitly sets the value selected by this model.
    /// </summary>
    /// <param name="value">The newly selected value.</param>
    /// <returns>True if the value was accepted, false if rejected.</returns>
    bool SetRawValue(object value);

    /// <summary>
    /// Convenience accessor to the value, throwing if set invalidly.
    /// </summary>
    object RawValue
    {
        get => GetRawValue();
        set
        {
            if (!SetRawValue(value))
                throw new ArgumentException($"Invalid value: {value}");
        }
    }
}
