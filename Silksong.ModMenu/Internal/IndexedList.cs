using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// A list that forces unique elements and indexes them for efficient containment checks.
/// </summary>
internal class IndexedList<T> : IList<T>
{
    private readonly List<T> list = [];
    private readonly Dictionary<T, int> lookup = [];

    public T this[int index]
    {
        get => list[index];
        set
        {
            if (index < 0 || index >= list.Count)
                throw new IndexOutOfRangeException($"{index} (Count: {list.Count})");

            if (lookup.TryGetValue(list[index], out var prev))
            {
                if (prev == index)
                    return;
                else
                    throw new ArgumentException($"Item already elsewhere in list");
            }

            lookup.Remove(list[index]);
            list[index] = value;
            lookup[value] = index;
        }
    }

    public int Count => list.Count;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        if (lookup.ContainsKey(item))
            throw new ArgumentException("Item already exists");

        list.Add(item);
        lookup[item] = list.Count - 1;
    }

    public void Clear()
    {
        list.Clear();
        lookup.Clear();
    }

    public bool Contains(T item) => lookup.ContainsKey(item);

    public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    public int IndexOf(T item) => lookup.TryGetValue(item, out var idx) ? idx : -1;

    public void Insert(int index, T item)
    {
        if (lookup.TryGetValue(item, out var idx))
        {
            if (index == idx)
                return;
            else
                throw new ArgumentException("Item already exists");
        }

        list.Insert(index, item);
        UpdateIndex(index);
    }

    public bool Remove(T item)
    {
        if (!lookup.TryGetValue(item, out var index))
            return false;

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index) => TryRemoveAt(index, out _);

    public bool TryRemoveAt(int index, [MaybeNullWhen(false)] out T item)
    {
        item = default;
        if (index < 0 || index >= list.Count)
            return false;

        item = list[index];
        list.RemoveAt(index);
        UpdateIndex(index);
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

    private void UpdateIndex(int from)
    {
        for (int i = from; i < list.Count; i++)
            lookup[list[i]] = i;
    }
}
