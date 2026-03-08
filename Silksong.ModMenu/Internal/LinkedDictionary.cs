using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Silksong.ModMenu.Internal;

internal class LinkedDictionary<K, V> : IDictionary<K, V>
{
    private readonly Dictionary<K, (LinkedListNode<K>, V)> data = [];
    private readonly LinkedList<K> keyOrder = [];

    public V this[K key]
    {
        get => data[key].Item2;
        set => data[key] = (data[key].Item1, value);
    }

    public ICollection<K> Keys => new LinkedDictionaryKeys(this);

    public ICollection<V> Values => new LinkedDictionaryValues(this);

    public int Count => keyOrder.Count;

    public bool IsReadOnly => false;

    public void Add(K key, V value) =>
        data[key] = (
            data.TryGetValue(key, out var pair) ? pair.Item1 : keyOrder.AddLast(key),
            value
        );

    public void Add(KeyValuePair<K, V> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        data.Clear();
        keyOrder.Clear();
    }

    public bool Contains(KeyValuePair<K, V> item) =>
        data.TryGetValue(item.Key, out var pair)
        && EqualityComparer<V>.Default.Equals(item.Value, pair.Item2);

    public bool ContainsKey(K key) => data.ContainsKey(key);

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        foreach (var key in keyOrder)
            array[arrayIndex++] = new(key, data[key].Item2);
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() =>
        keyOrder.Select(k => new KeyValuePair<K, V>(k, data[k].Item2)).GetEnumerator();

    public bool Remove(K key)
    {
        if (!data.TryGetValue(key, out var pair))
            return false;

        data.Remove(key);
        keyOrder.Remove(pair.Item1);
        return true;
    }

    public bool Remove(KeyValuePair<K, V> item)
    {
        if (
            !data.TryGetValue(item.Key, out var pair)
            || !EqualityComparer<V>.Default.Equals(item.Value, pair.Item2)
        )
            return false;

        data.Remove(item.Key);
        keyOrder.Remove(pair.Item1);
        return true;
    }

    public bool TryGetValue(K key, out V value)
    {
        if (!data.TryGetValue(key, out var pair))
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }

        value = pair.Item2;
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class LinkedDictionaryKeys(LinkedDictionary<K, V> parent) : ICollection<K>
    {
        public int Count => parent.Count;

        public bool IsReadOnly => false;

        public void Add(K item) => throw new InvalidOperationException();

        public void Clear() => parent.Clear();

        public bool Contains(K item) => parent.ContainsKey(item);

        public void CopyTo(K[] array, int arrayIndex)
        {
            foreach (var key in parent.keyOrder)
                array[arrayIndex++] = key;
        }

        public IEnumerator<K> GetEnumerator() => parent.keyOrder.GetEnumerator();

        public bool Remove(K item) => parent.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class LinkedDictionaryValues(LinkedDictionary<K, V> parent) : ICollection<V>
    {
        public int Count => parent.Count;

        public bool IsReadOnly => false;

        public void Add(V item) => throw new InvalidOperationException(nameof(Add));

        public void Clear() => parent.Clear();

        public bool Contains(V item) =>
            parent.data.Any(e => EqualityComparer<V>.Default.Equals(e.Value.Item2, item));

        public void CopyTo(V[] array, int arrayIndex)
        {
            foreach (var key in parent.keyOrder)
                array[arrayIndex++] = parent.data[key].Item2;
        }

        public IEnumerator<V> GetEnumerator() =>
            parent.keyOrder.Select(k => parent.data[k].Item2).GetEnumerator();

        public bool Remove(V item)
        {
            foreach (var e in parent.data)
            {
                if (!EqualityComparer<V>.Default.Equals(e.Value.Item2, item))
                    continue;

                parent.Remove(e.Key);
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
