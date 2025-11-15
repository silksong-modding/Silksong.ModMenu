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
    /// </summary>
    void UpdateLayout(Vector2 localAnchorPos);

    /// <summary>
    /// Set the visibility parent of this entity. Can only be done once.
    /// </summary>
    void SetMenuParent(IMenuEntity parent);

    /// <summary>
    /// Sets the GameObject container for this entity, from which all positions are relative. Can only be done once.
    /// </summary>
    void SetGameObjectParent(GameObject container);
}
