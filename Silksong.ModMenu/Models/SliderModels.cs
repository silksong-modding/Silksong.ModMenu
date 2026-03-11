using System.Collections.Generic;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A collection of helper functions for common types of SliderModels.
/// </summary>
public static class SliderModels
{
    /// <summary>
    /// A basic slider model for floats, with a minimum of 2 ticks (min, max). Intermediate values are interpolated linearly.
    /// </summary>
    public static LinearFloatSliderModel ForFloats(float min, float max, int ticks) =>
        new(min, max, ticks);

    /// <summary>
    /// A basic slider model with one tick per integer.
    /// </summary>
    public static IntSliderModel ForInts(int min, int max) => new(min, max);

    /// <summary>
    /// A slider model over an explicit list of values with custom names.
    /// </summary>
    public static ListSliderModel<T> ForNamedValues<T>(IEnumerable<(T, string)> values)
    {
        List<T> items = [];
        List<string> names = [];
        foreach (var (item, name) in values)
        {
            items.Add(item);
            names.Add(name);
        }

        return new(items) { DisplayFn = (idx, _) => names[idx] };
    }

    /// <summary>
    /// A slider model over an explicit list of values.
    /// </summary>
    public static ListSliderModel<T> ForValues<T>(List<T> values) => new(values);
}
