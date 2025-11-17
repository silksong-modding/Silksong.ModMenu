using System;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A simple choice model which steps 1 at a time between two integers.
/// </summary>
public class IntRangeChoiceModel : AbstractValueModel<int>, IChoiceModel<int>
{
    /// <summary>
    /// Construct a choice model with minimum and maximum integer constraints, both inclusive.
    /// </summary>
    public IntRangeChoiceModel(int min, int max, int value) => ResetParamsInternal(min, max, value);

    /// <summary>
    /// Minimum value for this model.
    /// </summary>
    public int Minimum { get; private set; }

    /// <summary>
    /// Maximum value for this model.
    /// </summary>
    public int Maximum { get; private set; }

    /// <summary>
    /// If true, wrap around when navigating past the max or min.
    /// </summary>
    public bool Circular = false;

    /// <summary>
    /// Update the parameters of this model atomically.
    /// </summary>
    public void ResetParams(int min, int max, int value)
    {
        var prev = this.value;
        ResetParamsInternal(min, max, value);

        if (this.value != prev)
            InvokeOnValueChanged();
    }

    private int value;

    /// <inheritdoc/>
    public override int GetValue() => value;

    /// <inheritdoc/>
    public bool MoveLeft()
    {
        if (value == Minimum)
            if (Circular)
                value = Maximum;
            else
                return false;
        else
            --value;

        InvokeOnValueChanged();
        return true;
    }

    /// <inheritdoc/>
    public bool MoveRight()
    {
        if (value == Maximum)
            if (Circular)
                value = Minimum;
            else
                return false;
        else
            ++value;

        InvokeOnValueChanged();
        return true;
    }

    /// <inheritdoc/>
    public override bool SetValue(int value)
    {
        if (this.value == value)
            return true;
        if (value < Minimum || value > Maximum)
            return false;

        this.value = value;
        InvokeOnValueChanged();
        return true;
    }

    /// <summary>
    /// A custom display function to use for the selected integer value.
    /// </summary>
    public Func<int, string>? DisplayFn;

    /// <inheritdoc/>
    public override string DisplayString() => DisplayFn?.Invoke(GetValue()) ?? $"{GetValue()}";

    private void ResetParamsInternal(int min, int max, int value)
    {
        if (max < min)
            throw new ArgumentException(
                $"{nameof(min)} ({min}) must be <= than {nameof(max)} ({max})"
            );

        Minimum = min;
        Maximum = max;
        this.value = Math.Clamp(value, min, max);
    }
}
