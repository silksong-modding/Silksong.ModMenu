using System;
using System.Collections.Generic;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// A recursive tree structure which supports lazy node creation and postfix traversal.
/// </summary>
internal class TreeNode<K, V>
    where V : new()
{
    private readonly Dictionary<K, TreeNode<K, V>> subtrees = [];

    public V Value = new();

    public IReadOnlyDictionary<K, TreeNode<K, V>> Subtrees => subtrees;

    public TreeNode<K, V> this[IEnumerable<K> keys]
    {
        get
        {
            TreeNode<K, V> tree = this;
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

    private void ForEachPostfixRecursive(
        List<K> keys,
        Action<IReadOnlyList<K>, TreeNode<K, V>> action
    )
    {
        foreach (var e in subtrees)
        {
            keys.Add(e.Key);
            e.Value.ForEachPostfixRecursive(keys, action);
            keys.RemoveAt(keys.Count - 1);
        }
        action(keys, this);
    }

    public void ForEachPostfix(Action<IReadOnlyList<K>, TreeNode<K, V>> action) =>
        ForEachPostfixRecursive([], action);
}
