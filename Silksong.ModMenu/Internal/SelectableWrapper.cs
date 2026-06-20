using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal class SelectableWrapper(Selectable selectable) : INavigable
{
    private readonly Selectable selectable = selectable;

    private Navigation Nav
    {
        get => selectable.navigation;
        set =>
            selectable.navigation = value with
            {
                mode = Navigation.Mode.Explicit,
                wrapAround = false,
            };
    }

    public void ClearNeighbors(NavigationDirection direction) =>
        Nav = direction switch
        {
            NavigationDirection.Up => Nav with { selectOnUp = null },
            NavigationDirection.Left => Nav with { selectOnLeft = null },
            NavigationDirection.Right => Nav with { selectOnRight = null },
            NavigationDirection.Down => Nav with { selectOnDown = null },
            _ => throw direction.UnsupportedEnum(),
        };

    public void ClearNeighbors() => Nav = new();

    public bool GetSelectables(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
    )
    {
        selectables = [selectable];
        return true;
    }

    public void SetNeighbors(NavigationDirection direction, IEnumerable<Selectable> selectables)
    {
        if (!selectables.Any())
            throw new ArgumentException(
                "At least one selectable is required.",
                nameof(selectables)
            );

        Vector2 thisPosition = selectable.transform.position;
        Vector2 directionVector = direction switch
        {
            NavigationDirection.Up => Vector2.up,
            NavigationDirection.Left => Vector2.left,
            NavigationDirection.Right => Vector2.right,
            NavigationDirection.Down => Vector2.down,
            _ => throw direction.UnsupportedEnum(),
        };

        Selectable bestOption = selectables
            .Select(selectable =>
            {
                Vector2 otherPosition = selectable.transform.position,
                    angleBetweenSelectables = otherPosition - thisPosition;

                float angleRelativeToDirection = Vector2.Angle(
                        directionVector,
                        angleBetweenSelectables
                    ),
                    angleRelativeToOpposite = Vector2.Angle(
                        -directionVector,
                        angleBetweenSelectables
                    );

                return (
                    selectable,
                    angleRelativeToDirection,
                    angleRelativeToAxis: Mathf.Min(
                        angleRelativeToDirection,
                        angleRelativeToOpposite
                    ),
                    position: otherPosition
                );
            })
            // Favour selectables on the correct side of this one, narrow down by how well aligned they are with the current selectable
            .OrderBy(other => (int)Mathf.RoundToMultipleOf(other.angleRelativeToDirection, 180))
            .ThenBy(other => (int)Mathf.RoundToMultipleOf(other.angleRelativeToAxis, 20))
            .ThenBy(other => Vector2.Distance(thisPosition, other.position))
            .First()
            .selectable;

        Nav = direction switch
        {
            NavigationDirection.Up => Nav with { selectOnUp = bestOption },
            NavigationDirection.Left => Nav with { selectOnLeft = bestOption },
            NavigationDirection.Right => Nav with { selectOnRight = bestOption },
            NavigationDirection.Down => Nav with { selectOnDown = bestOption },
            _ => throw direction.UnsupportedEnum(),
        };
    }
}
