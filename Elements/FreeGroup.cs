using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// An arbitrary collection of menu elements where each has a manually assigned position.
/// Navigation to defaults to the single left/right/top/bottom-most element within.
/// </summary>
public class FreeGroup : INavigableMenuEntity
{
    private readonly VisibilityManager visibility = new();
    private readonly LinkedDictionary<IMenuEntity, Vector2> entities = [];

    /// <inheritdoc/>
    public VisibilityManager Visibility => visibility;

    /// <inheritdoc/>
    public IEnumerable<MenuElement> AllElements() => entities.Keys.SelectMany(e => e.AllElements());

    /// <inheritdoc/>
    public void ClearNeighbor(NavigationDirection direction)
    {
        if (!GetSelectable(direction, out var selectable))
            return;

        SelectableWrapper wrapper = new(selectable);
        wrapper.ClearNeighbor(direction);
    }

    /// <inheritdoc/>
    public void ClearNeighbors()
    {
        ClearNeighbor(NavigationDirection.Up);
        ClearNeighbor(NavigationDirection.Left);
        ClearNeighbor(NavigationDirection.Right);
        ClearNeighbor(NavigationDirection.Down);
    }

    /// <inheritdoc/>
    public SelectableElement? GetDefaultSelectable() =>
        entities
            .Keys.OfType<INavigableMenuEntity>()
            .Select(e => e.GetDefaultSelectable())
            .WhereNonNull()
            .FirstOrDefault();

    /// <summary>
    /// Add an entity to this free group at the specified position.
    /// </summary>
    public void Add(IMenuEntity entity, Vector2 offset)
    {
        entities.Add(entity, offset);
        entity.SetMenuParent(this);

        if (gameObjectParent != null)
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <summary>
    /// Update the specified position of the given entity.
    /// </summary>
    public void Update(IMenuEntity entity, Vector2 offset)
    {
        if (!entities.ContainsKey(entity))
            throw new ArgumentException($"Entity not present in FreeGroup");

        entities[entity] = offset;
    }

    private static float SortKey(NavigationDirection direction, Vector2 pos) =>
        direction switch
        {
            NavigationDirection.Up => -pos.y,
            NavigationDirection.Left => pos.x,
            NavigationDirection.Right => -pos.x,
            NavigationDirection.Down => pos.y,
            _ => throw new ArgumentException($"{direction}"),
        };

    /// <inheritdoc/>
    public bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    )
    {
        selectable = entities
            .OrderBy(e => SortKey(direction, e.Value))
            .Select(e =>
                ((e.Key as INavigable)?.GetSelectable(direction, out var s) ?? false) ? s : null
            )
            .WhereNonNull()
            .FirstOrDefault();
        return selectable != null;
    }

    private GameObject? gameObjectParent;

    /// <inheritdoc/>
    public void SetGameObjectParent(GameObject parent)
    {
        if (gameObjectParent != null)
            throw new ArgumentException("GameObjectParent already set");

        gameObjectParent = parent;
        foreach (var entity in entities.Keys)
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <inheritdoc/>
    public void SetMenuParent(IMenuEntity parent) => visibility.SetParent(parent.Visibility);

    /// <inheritdoc/>
    public void SetNeighbor(NavigationDirection direction, Selectable selectable)
    {
        if (!GetSelectable(direction, out var mine))
            return;

        SelectableWrapper wrapper = new(mine);
        wrapper.SetNeighbor(direction, selectable);
    }

    /// <inheritdoc/>
    public void UpdateLayout(Vector2 localAnchorPos)
    {
        ClearNeighbors();
        foreach (var e in entities)
            e.Key.UpdateLayout(localAnchorPos + e.Value);
    }
}
