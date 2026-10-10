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
public abstract class AbstractGroup : MenuDisposable, INavigableMenuEntity
{
    private readonly VisibilityManager visibility = new(false);

    /// <summary>
    /// Construct an AbstractGroup.
    /// </summary>
    protected AbstractGroup()
    {
        OnDispose += () =>
        {
            foreach (var disposable in AllEntities().OfType<MenuDisposable>())
                disposable.Dispose();
        };
    }

    /// <inheritdoc/>
    public VisibilityManager Visibility => visibility;

    /// <summary>
    /// Enumerate all child entities within this group.
    /// </summary>
    public abstract IEnumerable<IMenuEntity> AllEntities();

    /// <inheritdoc/>
    public IEnumerable<MenuElement> AllElements() => AllEntities().SelectMany(e => e.AllElements());

    /// <summary>
    /// Whether this entity is directly contained within thie group.
    /// </summary>
    public abstract bool Contains(IMenuEntity entity);

    /// <summary>
    /// Register `entity` as a child of this group.
    /// </summary>
    protected void AddChild(IMenuEntity entity) => entity.SetParents(this, gameObjectParent);

    /// <summary>
    /// Clear all entities from this group.
    ///
    /// This does not destroy the corresponding game objects. The caller should call `Dispose()` on the entities after `Clear()` if they are no longer wanted.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Enumerate all navigables which should be directly connected in `direction`.
    /// </summary>
    protected abstract IEnumerable<INavigable> GetNavigables(NavigationDirection direction);

    /// <inheritdoc/>
    public virtual void ClearNeighbors(NavigationDirection direction)
    {
        foreach (var navigable in GetNavigables(direction))
            navigable.ClearNeighbors(direction);
    }

    /// <inheritdoc/>
    public virtual void ClearNeighbors()
    {
        ClearNeighbors(NavigationDirection.Up);
        ClearNeighbors(NavigationDirection.Left);
        ClearNeighbors(NavigationDirection.Right);
        ClearNeighbors(NavigationDirection.Down);
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
    public abstract bool GetSelectables(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
    );

    private GameObject? gameObjectParent;

    /// <inheritdoc/>
    public virtual void SetGameObjectParent(GameObject parent)
    {
        ClearGameObjectParent();

        gameObjectParent = parent;
        foreach (var entity in AllEntities())
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <inheritdoc/>
    public virtual void ClearGameObjectParent()
    {
        if (gameObjectParent == null)
            return;

        foreach (var entity in AllEntities())
            entity.ClearGameObjectParent();
    }

    /// <inheritdoc/>
    public virtual void SetNeighbors(
        NavigationDirection direction,
        IEnumerable<Selectable> selectables
    )
    {
        foreach (var navigable in GetNavigables(direction))
            navigable.SetNeighbors(direction, selectables);
    }

    /// <inheritdoc/>
    public abstract void UpdateLayout(Vector2 localAnchorPos);
}
