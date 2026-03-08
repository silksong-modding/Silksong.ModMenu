using System.Collections.Generic;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A basic ValueModel that implements no other interfaces.
/// </summary>
public class ValueModel<T>(T value) : AbstractValueModel<T>
{
    private T value = value;

    /// <inheritdoc/>
    public override T GetValue() => value;

    /// <inheritdoc/>
    public override bool SetValue(T value)
    {
        if (EqualityComparer<T>.Default.Equals(this.value, value))
            return true;

        this.value = value;
        InvokeOnValueChanged();
        return true;
    }
}
