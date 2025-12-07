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
public class FreeGroup : AbstractGroup
{
    private readonly LinkedDictionary<IMenuEntity, Vector2> entities = [];

    /// <inheritdoc/>
    protected override IEnumerable<IMenuEntity> AllEntities() => entities.Keys;

    /// <summary>
    /// Add an entity to this free group at the specified position.
    /// </summary>
    public void Add(IMenuEntity entity, Vector2 offset)
    {
        entities[entity] = offset;
        AddChild(entity);
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

    /// <summary>
    /// Remove the specified entity from this group.
    /// </summary>
    public bool Remove(IMenuEntity entity)
    {
        if (entities.Remove(entity))
        {
            entity.ClearParents();
            return true;
        }

        return false;
    }

    // Sort low values first.
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
    public override bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    )
    {
        selectable = GetNavigables(direction)
            .Select(n => n.GetSelectable(direction, out var s) ? s : null)
            .FirstOrDefault();
        return selectable != null;
    }

    /// <inheritdoc/>
    protected override IEnumerable<INavigable> GetNavigables(NavigationDirection direction)
    {
        var validEntities = entities.Where(e => e.Key.VisibleSelf && e.Key is INavigable);
        if (!validEntities.Select(e => SortKey(direction, e.Value)).TryGetMin(out var key))
            return [];

        return validEntities
            .Where(e => SortKey(direction, e.Value) == key)
            .Select(e => e.Key as INavigable)!
            .Take(1);
    }

    /// <inheritdoc/>
    public override void UpdateLayout(Vector2 localAnchorPos)
    {
        ClearNeighbors();
        foreach (var e in entities)
            e.Key.UpdateLayout(localAnchorPos + e.Value);
    }
}
