using System;
using UnityEngine;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A float slider which linearly interpolates between the given minimum and maximum.
/// It is not required that min < max; the two can be inverted for a reversed slider.
/// </summary>
public class LinearFloatSliderModel : SliderModel<float>
{
    public LinearFloatSliderModel(float min, float max, int ticks)
        : base(0, ticks)
    {
        if (ticks < 2)
            throw new ArgumentException($"{nameof(ticks)} ({ticks}) must be positive");
        if (MathF.Abs(MaximumValue - MinimumValue) < 1e-6f)
            throw new ArgumentException(
                $"{nameof(min)} ({min}) and {nameof(max)} ({max}) are too close."
            );

        MinimumValue = min;
        MaximumValue = max;
    }

    public readonly float MinimumValue;

    public readonly float MaximumValue;

    public readonly int Ticks;

    /// <inheritdoc/>
    protected override float GetValue(int index) =>
        MinimumValue + (MaximumValue - MinimumValue) * (index * 1f / (Ticks - 1));

    /// <inheritdoc/>
    protected override bool GetIndex(float value, out int index)
    {
        index = -1;

        var (min, max) = (MinimumValue, MaximumValue);
        bool reverse = MinimumValue > MaximumValue;
        if (reverse)
            (min, max) = (max, min);

        if (value < min || value > max)
            return false;

        float idx = (value - min) * (Ticks - 1) / (max - min);
        index = Mathf.RoundToInt(idx);
        if (reverse)
            index = Ticks - 1 - index;
        return true;
    }

    protected override string DefaultDisplayString(int index, float item) => $"{item:0.###}";
}
