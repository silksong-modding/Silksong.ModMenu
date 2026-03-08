using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A single column of menu elements, evenly spaced.
/// </summary>
public class VerticalGroup : AbstractGroup
{
    private readonly IndexedList<IMenuEntity> entities = [];

    /// <summary>
    /// Vertical space between rendered elements.
    /// </summary>
    public float VerticalSpacing = SpacingConstants.VSPACE_MEDIUM;

    /// <summary>
    /// If true, hide move lower elements upwards when space is made for them by inactive elements above.
    /// </summary>
    public bool HideInactiveElements = true;

    /// <inheritdoc/>
    protected override IEnumerable<IMenuEntity> AllEntities() => entities;

    /// <summary>
    /// Add an entity to this vertical column group.
    /// </summary>
    public void Add(IMenuEntity entity)
    {
        entities.Add(entity);
        AddChild(entity);
    }

    /// <summary>
    /// Add a collection of entities to this vertical column group.
    /// </summary>
    public void AddRange(IEnumerable<IMenuEntity> entities)
    {
        foreach (var entity in entities)
            Add(entity);
    }

    /// <summary>
    /// Insert the entity at the specified index.
    /// </summary>
    public void Insert(int index, IMenuEntity entity)
    {
        entities.Insert(index, entity);
        AddChild(entity);
    }

    /// <summary>
    /// Remove the specified entity from this layout group.
    /// </summary>
    public bool Remove(IMenuEntity entity)
    {
        if (!entities.Remove(entity))
            return false;

        entity.ClearParents();
        return true;
    }

    /// <summary>
    /// Remove the entity at the specified index.
    /// </summary>
    public void RemoveAt(int index)
    {
        if (entities.TryRemoveAt(index, out var entity))
            entity.ClearParents();
    }

    /// <inheritdoc/>
    public override bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    )
    {
        switch (direction)
        {
            case NavigationDirection.Left:
            case NavigationDirection.Right:
                selectable = NonHiddenEntities()
                    .MedianOutwards()
                    .OfType<INavigable>()
                    .Select(n => n.GetSelectable(direction, out var s) ? s : null)
                    .FirstOrDefault();
                return selectable != null;
            case NavigationDirection.Up:
                selectable = NonHiddenEntities()
                    .OfType<INavigable>()
                    .Select(n => n.GetSelectable(direction, out var s) ? s : null)
                    .LastOrDefault();
                return selectable != null;
            case NavigationDirection.Down:
                selectable = NonHiddenEntities()
                    .OfType<INavigable>()
                    .Select(n => n.GetSelectable(direction, out var s) ? s : null)
                    .FirstOrDefault();
                return selectable != null;
            default:
                throw direction.UnsupportedEnum();
        }
    }

    /// <inheritdoc/>
    public override void UpdateLayout(Vector2 pos)
    {
        ClearNeighbors();
        foreach (var entity in NonHiddenEntities())
        {
            entity.UpdateLayout(pos);
            pos.y -= VerticalSpacing;
        }

        foreach (var (top, bot) in NonHiddenEntities().OfType<INavigable>().Pairs())
            top.ConnectSymmetric(bot, NavigationDirection.Down);
    }

    private IEnumerable<IMenuEntity> NonHiddenEntities() =>
        HideInactiveElements ? entities.Where(e => e.VisibleSelf) : entities;

    /// <inheritdoc/>
    protected override IEnumerable<INavigable> GetNavigables(NavigationDirection direction)
    {
        return direction switch
        {
            NavigationDirection.Left or NavigationDirection.Right => NonHiddenEntities()
                .OfType<INavigable>(),
            NavigationDirection.Up => NonHiddenEntities().OfType<INavigable>().Take(1),
            NavigationDirection.Down => NonHiddenEntities().OfType<INavigable>().Reverse().Take(1),
            _ => throw direction.UnsupportedEnum(),
        };
    }
}
