using System;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A model for a selected tick on a slider.
/// </summary>
public abstract class SliderModel<T> : AbstractValueModel<T>
{
    /// <summary>
    /// Constructs a new slider model for a given range.
    /// </summary>
    /// <param name="min">Inclusive minimum bound, must be &lt; max.</param>
    /// <param name="max">Inclusive maximum bound, must be *gt; min.</param>
    protected SliderModel(int min, int max)
    {
        if (max <= min)
            throw new ArgumentException($"Min ({min}) should be < Max ({max})");

        MinimumIndex = min;
        MaximumIndex = max;
        Index = min;
    }

    /// <summary>
    /// The minimum index value for this slider model.
    /// </summary>
    public readonly int MinimumIndex;

    /// <summary>
    /// The maximum index value for this slider model.
    /// </summary>
    public readonly int MaximumIndex;

    private int _index;

    /// <summary>
    /// The currently selected index by this model.
    /// </summary>
    public int Index
    {
        get => _index;
        set
        {
            if (value < MinimumIndex || value > MaximumIndex)
                throw new ArgumentOutOfRangeException(
                    $"SelectedIndex ({value}) out of range: [{MinimumIndex}, {MaximumIndex}]"
                );
            if (_index == value)
                return;

            _index = value;
            InvokeOnValueChanged();
        }
    }

    /// <summary>
    /// Get the value associated with this specific index.
    /// </summary>
    protected abstract T GetValue(int index);

    /// <summary>
    /// Get the index associated witht his specific value, or return false if none applies.
    /// </summary>
    protected abstract bool GetIndex(T value, out int index);

    /// <summary>
    /// Get the value for the currently selected index.
    /// </summary>
    public override T GetValue() => GetValue(Index);

    /// <summary>
    /// Set the selected index by value.
    /// </summary>
    public override bool SetValue(T value)
    {
        if (!GetIndex(value, out var idx))
            return false;
        if (idx < MinimumIndex || idx > MaximumIndex)
            return false;

        Index = idx;
        return true;
    }

    /// <summary>
    /// An optional custom display name function.
    /// </summary>
    public IndexedToString<T>? DisplayFn;

    /// <inheritdoc/>
    public override LocalizedText DisplayString() =>
        (DisplayFn ?? DefaultDisplayString).Invoke(Index, GetValue());

    /// <summary>
    /// The default display string for this model, in the absence of a DisplayFn.
    /// </summary>
    protected virtual LocalizedText DefaultDisplayString(int index, T item) => $"{item}";
}
