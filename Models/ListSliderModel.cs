using System;
using System.Collections.Generic;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A slider model operating on an explicit list of elements. Requires at least 2 elements.
/// </summary>
public class ListSliderModel<T>(List<T> items) : SliderModel<T>(0, items.Count - 1)
{
    private readonly T[] items = [.. items];

    protected override bool GetIndex(T value, out int index)
    {
        index = Array.IndexOf(items, value);
        return index >= 0;
    }

    protected override T GetValue(int index) => items[index];
}
