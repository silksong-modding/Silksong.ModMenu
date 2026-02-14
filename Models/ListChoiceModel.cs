using System;
using System.Collections.Generic;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A choice model over the domain of a finite list of unique values.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="values">The finite list of values this model may choose.</param>
/// <remarks>
/// Build a new ListChoiceModel. This copies the input list.
/// Requires at least one element in the list.
/// </remarks>
public class ListChoiceModel<T>(List<T> values) : AbstractValueModel<T>, IChoiceModel<T>
{
    /// <summary>
    /// The list of values that may be chosen.
    /// </summary>
    public IReadOnlyList<T> Values { get; private set; } =
        values.Count > 0 ? [.. values] : throw new ArgumentException($"Empty list");

    /// <summary>
    /// The number of values that may be chosen.
    /// </summary>
    public int Count => Values.Count;

    /// <summary>
    /// If true, the list loops on itself when pushed right past the end. Otherwise, it halts at both ends.
    /// </summary>
    public bool Circular = true;

    /// <summary>
    /// Change the list of values held within this model and the selected index, atomically.
    /// </summary>
    public void UpdateValues(List<T> newValues, int selectedIndex)
    {
        if (newValues.Count == 0)
            throw new ArgumentException($"{nameof(newValues)} cannot be empty");
        if (selectedIndex < 0 || selectedIndex >= newValues.Count)
            throw new ArgumentOutOfRangeException(
                $"{nameof(selectedIndex)} ({selectedIndex}) is out of range: [0, {newValues.Count})"
            );

        Values = [.. newValues];
        _index = selectedIndex;
        InvokeOnValueChanged();
    }

    private int _index;

    /// <summary>
    /// The index of the currently selected value.
    /// </summary>
    public int Index
    {
        get => _index;
        set
        {
            if (value < 0 || value >= Values.Count)
                throw new IndexOutOfRangeException(
                    $"Index ({value}) out of range: [0, {Values.Count})"
                );
            if (_index == value)
                return;

            _index = value;
            InvokeOnValueChanged();
        }
    }

    /// <inheritdoc/>
    public override T GetValue() => Values[Index];

    /// <inheritdoc/>
    public override bool SetValue(T value)
    {
        if (EqualityComparer<T>.Default.Equals(value, GetValue()))
            return true;

        for (int i = 0; i < Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(Values[i], value))
            {
                Index = i;
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public bool MoveLeft() => Move(-1);

    /// <inheritdoc/>
    public bool MoveRight() => Move(1);

    /// <summary>
    /// An optional custom display name function.
    /// </summary>
    public IndexedToString<T>? DisplayFn;

    /// <inheritdoc/>
    public override LocalizedText DisplayString() =>
        (DisplayFn ?? DefaultDisplayString).Invoke(Index, GetValue());

    /// <summary>
    /// Default string to display, in the absence of a DisplayFn.
    /// </summary>
    protected virtual LocalizedText DefaultDisplayString(int index, T item) => $"{item}";

    private bool Move(int delta)
    {
        var newIndex = Index + delta;
        if (newIndex < 0)
            newIndex = Circular ? newIndex % Count + Count : 0;
        if (newIndex >= Count)
            newIndex = Circular ? newIndex % Count : Count - 1;

        if (Index == newIndex)
            return false;

        Index = newIndex;
        return true;
    }
}
