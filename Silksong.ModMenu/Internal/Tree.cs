using System;
using System.Collections.Generic;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// A recursive tree structure which supports lazy node creation and postfix traversal.
/// </summary>
internal class Tree<K, V>
    where V : new()
{
    private readonly Dictionary<K, Tree<K, V>> subtrees = [];

    public V Value = new();

    public IReadOnlyDictionary<K, Tree<K, V>> Subtrees => subtrees;

    public Tree<K, V> this[IEnumerable<K> keys]
    {
        get
        {
            Tree<K, V> tree = this;
            foreach (var key in keys)
            {
                if (tree.subtrees.TryGetValue(key, out var subtree))
                    tree = subtree;
                else
                {
                    subtree = new();
                    tree.subtrees[key] = subtree;
                    tree = subtree;
                }
            }
            return tree;
        }
    }

    private void ForEachPostfixRecursive(List<K> keys, Action<IReadOnlyList<K>, Tree<K, V>> action)
    {
        foreach (var e in subtrees)
        {
            keys.Add(e.Key);
            e.Value.ForEachPostfixRecursive(keys, action);
            keys.RemoveAt(keys.Count - 1);
        }
        action(keys, this);
    }

    public void ForEachPostfix(Action<IReadOnlyList<K>, Tree<K, V>> action) =>
        ForEachPostfixRecursive([], action);
}
