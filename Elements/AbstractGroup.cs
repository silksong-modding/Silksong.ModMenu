using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Common functionality shared by most entity groups.
/// </summary>
public abstract class AbstractGroup : INavigableMenuEntity
{
    private readonly VisibilityManager visibility = new();

    /// <inheritdoc/>
    public VisibilityManager Visibility => visibility;

    /// <summary>
    /// Enumerate all child entities within this group.
    /// </summary>
    protected abstract IEnumerable<IMenuEntity> AllEntities();

    /// <inheritdoc/>
    public IEnumerable<MenuElement> AllElements() => AllEntities().SelectMany(e => e.AllElements());

    /// <summary>
    /// Record `entity` as a child of this group. The group is responsible for storing it in whatever data structure makes sense for it.
    /// </summary>
    protected void ParentEntity(IMenuEntity entity)
    {
        entity.SetMenuParent(this);
        if (gameObjectParent != null)
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <summary>
    /// Enumerate all navigables which should be directly connected in `direction`.
    /// </summary>
    protected abstract IEnumerable<INavigable> GetNavigables(NavigationDirection direction);

    /// <inheritdoc/>
    public virtual void ClearNeighbor(NavigationDirection direction)
    {
        foreach (var navigable in GetNavigables(direction))
            navigable.ClearNeighbor(direction);
    }

    /// <inheritdoc/>
    public virtual void ClearNeighbors()
    {
        ClearNeighbor(NavigationDirection.Up);
        ClearNeighbor(NavigationDirection.Left);
        ClearNeighbor(NavigationDirection.Right);
        ClearNeighbor(NavigationDirection.Down);
    }

    /// <inheritdoc/>
    public virtual SelectableElement? GetDefaultSelectable() =>
        AllEntities()
            .Where(e => e.VisibleSelf)
            .OfType<INavigableMenuEntity>()
            .Select(n => n.GetDefaultSelectable())
            .WhereNonNull()
            .FirstOrDefault();

    /// <inheritdoc/>
    public abstract bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    );

    private GameObject? gameObjectParent;

    /// <inheritdoc/>
    public virtual void SetGameObjectParent(GameObject parent)
    {
        if (gameObjectParent != null)
            throw new ArgumentException("GameObjectParent already set");

        gameObjectParent = parent;
        foreach (var entity in AllEntities())
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <inheritdoc/>
    public virtual void SetMenuParent(IMenuEntity parent) =>
        visibility.SetParent(parent.Visibility);

    /// <inheritdoc/>
    public virtual void SetNeighbor(NavigationDirection direction, Selectable selectable)
    {
        foreach (var navigable in GetNavigables(direction))
            navigable.SetNeighbor(direction, selectable);
    }

    /// <inheritdoc/>
    public abstract void UpdateLayout(Vector2 localAnchorPos);
}
