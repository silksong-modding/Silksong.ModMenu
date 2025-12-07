using System.Collections.Generic;
using UnityEngine;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// An abstraction for a singular menu element, a container of them, or a collection of menu elements.
/// </summary>
public interface IMenuEntity
{
    /// <summary>
    /// Controls whether this entity is visible or not.
    /// </summary>
    VisibilityManager Visibility { get; }

    /// <summary>
    /// Return all atomic MenuElements recursively.
    /// </summary>
    IEnumerable<MenuElement> AllElements();

    /// <summary>
    /// Update the positioning of this element and all nested elements.
    ///
    /// Implementations of UpdateLayout should:
    ///   1) Clear all navigation.
    ///   2) Invoke UpdateLayout recursively on children (DFS).
    ///   3) Reset navigation at the current layer.
    ///
    /// This convention ensures that different container types will play nicely when nested inside each other.
    /// </summary>
    void UpdateLayout(Vector2 localAnchorPos);

    /// <summary>
    /// Sets the GameObject container for this entity, from which all positions are relative.
    /// </summary>
    void SetGameObjectParent(GameObject container);

    /// <summary>
    /// Make this entity parent-less, which in most cases also renders it invisible.
    /// </summary>
    void ClearGameObjectParent();
}
