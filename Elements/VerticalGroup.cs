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
public class VerticalGroup : INavigableMenuEntity
{
    private readonly VisibilityManager visibility = new();
    private readonly List<IMenuEntity> entities = [];

    /// <summary>
    /// Offset for the top center anchor point of the elements, relative to the parent container.
    /// </summary>
    public Vector2 Offset = SpacingConstants.TOP_CENTER_ANCHOR;

    /// <summary>
    /// Vertical space between rendered elements.
    /// </summary>
    public float VerticalSpacing = SpacingConstants.VSPACE_MEDIUM;

    /// <summary>
    /// If true, hide move lower elements upwards when space is made for them by inactive elements above.
    /// </summary>
    public bool HideInactiveElements = true;

    /// <inheritdoc/>
    public VisibilityManager Visibility => visibility;

    public IEnumerable<MenuElement> AllElements() => entities.SelectMany(e => e.AllElements());

    public SelectableElement? GetDefaultSelectable() =>
        NonHiddenEntities()
            .OfType<INavigableMenuEntity>()
            .Select(e => e.GetDefaultSelectable())
            .Where(s => s != null)
            .FirstOrDefault();

    /// <summary>
    /// Add an entity to this vertical column group.
    /// </summary>
    public void Add(IMenuEntity entity)
    {
        entities.Add(entity);
        entity.SetMenuParent(this);

        if (gameObjectParent != null)
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <summary>
    /// Add a collection of entities to this vertical column group.
    /// </summary>
    public void AddRange(IEnumerable<IMenuEntity> entities)
    {
        foreach (var entity in entities)
            Add(entity);
    }

    /// <inheritdoc/>
    public void ClearNeighbor(NavigationDirection direction)
    {
        foreach (var neighbor in GetNavigables(direction))
            neighbor.ClearNeighbor(direction);
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
    public bool GetSelectable(
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
    public void SetNeighbor(NavigationDirection direction, Selectable selectable)
    {
        foreach (var navigable in GetNavigables(direction))
            navigable.SetNeighbor(direction, selectable);
    }

    /// <inheritdoc/>
    public void UpdateLayout(Vector2 pos)
    {
        foreach (var entity in NonHiddenEntities())
        {
            entity.UpdateLayout(pos);
            pos.y -= VerticalSpacing;
        }

        foreach (var navigable in NonHiddenEntities().OfType<INavigable>())
            navigable.ClearNeighbors();
        foreach (var (top, bot) in NonHiddenEntities().OfType<INavigable>().Pairs())
        {
            if (bot.GetSelectable(NavigationDirection.Down, out var s))
                top.SetNeighborDown(s);
            if (top.GetSelectable(NavigationDirection.Up, out s))
                bot.SetNeighborUp(s);
        }
    }

    /// <inheritdoc/>
    public void SetMenuParent(IMenuEntity parent) => visibility.SetParent(parent.Visibility);

    private GameObject? gameObjectParent;

    /// <inheritdoc/>
    public void SetGameObjectParent(GameObject parent)
    {
        if (gameObjectParent != null)
            throw new ArgumentException("GameObjectParent already set");

        gameObjectParent = parent;
        foreach (var entity in entities)
            entity.SetGameObjectParent(gameObjectParent);
    }

    private IEnumerable<IMenuEntity> NonHiddenEntities() =>
        HideInactiveElements ? entities.Where(e => e.VisibleSelf) : entities;

    private IEnumerable<INavigable> GetNavigables(NavigationDirection direction)
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
