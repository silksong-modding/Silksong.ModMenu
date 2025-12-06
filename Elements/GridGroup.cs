using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A fixed-width grid with a specific number of columns and an unbounded number of rows.
/// </summary>
public class GridGroup(int columns) : INavigableMenuEntity
{
    private readonly VisibilityManager visibility = new();
    private readonly List<IMenuEntity?[]> entitiesByRow = [];

    /// <summary>
    /// The number of columns in this grid.
    /// </summary>
    public readonly int Columns =
        columns > 0 ? columns : throw new ArgumentException($"Columns: {columns}");

    /// <summary>
    /// The number of rows in this grid.
    /// </summary>
    public int Rows => entitiesByRow.Count;

    /// <summary>
    /// Spacing between different columns.
    /// </summary>
    public float HorizontalSpacing = SpacingConstants.HSPACE_MEDIUM;

    /// <summary>
    /// Spacing between different rows.
    /// </summary>
    public float VerticalSpacing = SpacingConstants.VSPACE_MEDIUM;

    /// <summary>
    /// If true, make navigation wrap from right to left.
    /// </summary>
    public bool WrapHorizontal = false;

    /// <inheritdoc/>
    public VisibilityManager Visibility => visibility;

    /// <inheritdoc/>
    public IEnumerable<MenuElement> AllElements() => AllEntities().SelectMany(e => e.AllElements());

    /// <summary>
    /// Add this entity to the next available empty cell in the grid.
    /// </summary>
    public void Add(IMenuEntity entity)
    {
        while (IsFull(nextEmpty))
            nextEmpty = nextEmpty.Next(this);

        AddAt(nextEmpty.Row, nextEmpty.Column, entity);
        nextEmpty = nextEmpty.Next(this);
    }

    /// <summary>
    /// Add this entity to the specified cell in the grid.
    /// </summary>
    /// <param name="row">0-based row index.</param>
    /// <param name="column">0-based column index.</param>
    /// <param name="entity">The entity to add.</param>
    public void AddAt(int row, int column, IMenuEntity entity)
    {
        if (row < 0)
            throw new ArgumentException($"{nameof(row)}: {row} (Must be >= 0)");
        if (column < 0 || column >= Columns)
            throw new ArgumentException($"{nameof(column)}: {column} (Must be in [0, {Columns}))");
        if (IsFull(new(row, column)))
            throw new ArgumentException($"Cell({row}, {column}) is already filled.");

        while (entitiesByRow.Count <= row)
            entitiesByRow.Add(new IMenuEntity?[Columns]);
        entitiesByRow[row][column] = entity;

        entity.SetMenuParent(this);
        if (gameObjectParent != null)
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <inheritdoc/>
    public void ClearNeighbor(NavigationDirection direction)
    {
        foreach (var navigable in GetNavigables(direction))
            navigable.ClearNeighbor(direction);
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
        entitiesByRow
            .SelectMany(row => row.OfType<INavigableMenuEntity>())
            .Select(n => n.GetDefaultSelectable())
            .WhereNonNull()
            .FirstOrDefault();

    /// <inheritdoc/>
    public bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    )
    {
        var navigable = direction switch
        {
            // Last element.
            NavigationDirection.Up => AllEntities()
                .Where(e => e.VisibleSelf)
                .OfType<INavigable>()
                .LastOrDefault(),
            // Rightmost element.
            NavigationDirection.Left => GetColumns()
                .SelectMany(col => col.WhereNonNull(e => e.VisibleSelf).OfType<INavigable>())
                .LastOrDefault(),
            // Leftmost element.
            NavigationDirection.Right => GetColumns()
                .SelectMany(col => col.WhereNonNull(e => e.VisibleSelf).OfType<INavigable>())
                .FirstOrDefault(),
            // First element.
            NavigationDirection.Down => AllEntities()
                .Where(e => e.VisibleSelf)
                .OfType<INavigable>()
                .FirstOrDefault(),
            _ => throw new ArgumentException($"{direction}"),
        };

        selectable = default;
        return navigable != null && navigable.GetSelectable(direction, out selectable);
    }

    private GameObject? gameObjectParent;

    /// <inheritdoc/>
    public void SetGameObjectParent(GameObject parent)
    {
        if (gameObjectParent != null)
            throw new ArgumentException("GameObjectParent already set");

        gameObjectParent = parent;
        foreach (var entity in AllEntities())
            entity.SetGameObjectParent(gameObjectParent);
    }

    /// <inheritdoc/>
    public void SetMenuParent(IMenuEntity parent) => visibility.SetParent(parent.Visibility);

    /// <inheritdoc/>
    public void SetNeighbor(NavigationDirection direction, Selectable selectable)
    {
        foreach (var navigable in GetNavigables(direction))
            navigable.SetNeighbor(direction, selectable);
    }

    /// <inheritdoc/>
    public void UpdateLayout(Vector2 localAnchorPos)
    {
        ClearNeighbors();

        // Update positions.
        for (int row = 0; row < entitiesByRow.Count; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                var entity = entitiesByRow[row][column];
                if (entity == null)
                    continue;

                Vector2 pos = localAnchorPos;
                pos.y -= VerticalSpacing * row;
                pos.x += HorizontalSpacing * (column - (Columns - 1) / 2f);
                entity.UpdateLayout(pos);
            }
        }

        // Update navigation.
        INavigable?[]? prevRow = null;
        foreach (var row in entitiesByRow)
        {
            INavigable?[] nextRow =
            [
                .. row.Select(e => (e?.VisibleSelf ?? false) ? e as INavigable : null),
            ];
            if (!nextRow.Any(n => n != null))
                continue;

            if (prevRow != null)
            {
                // Find the closest valid selectable to the specified column, in an adjacent row.
                static bool ClosestColumn(
                    INavigable?[] row,
                    NavigationDirection dir,
                    int column,
                    [MaybeNullWhen(false)] out Selectable target
                )
                {
                    int offset = 0;
                    while (offset < row.Length)
                    {
                        int idx = column + (dir == NavigationDirection.Down ? offset : -offset);

                        // Count pos+neg outwards.
                        if (offset == 0)
                            offset = 1;
                        else if (offset > 0)
                            offset = -offset;
                        else
                            offset = 1 - offset;

                        if (
                            idx >= 0
                            && idx < row.Length
                            && (row[idx]?.GetSelectable(dir, out target) ?? false)
                        )
                            return true;
                    }

                    target = default;
                    return false;
                }

                for (int i = 0; i < Columns; i++)
                {
                    if (
                        prevRow[i] != null
                        && ClosestColumn(nextRow, NavigationDirection.Down, i, out var s)
                    )
                        prevRow[i]!.SetNeighborDown(s);
                    if (
                        nextRow[i] != null
                        && ClosestColumn(prevRow, NavigationDirection.Up, i, out s)
                    )
                        nextRow[i]!.SetNeighborUp(s);
                }
            }
            prevRow = nextRow;

            // Connect columns.
            foreach (var (left, right) in (WrapHorizontal ? nextRow.WhereNonNull().CircularPairs() : nextRow.WhereNonNull().Pairs()))
                NavigationDirection.Right.ConnectPair(left, right);
        }
    }

    private IEnumerable<IMenuEntity> AllEntities() =>
        entitiesByRow.SelectMany(row => row.WhereNonNull());

    private ListView<ListView<IMenuEntity?>> GetColumns() =>
        new(column => new(row => entitiesByRow[row][column], entitiesByRow.Count), Columns);

    private IEnumerable<INavigable> GetNavigables(NavigationDirection direction) =>
        direction switch
        {
            // All elements of first row with stuff in it.
            NavigationDirection.Up => entitiesByRow
                .Where(row => row.Any(e => e is INavigable && e.VisibleSelf))
                .FirstOrDefault()
                ?.OfType<INavigable>()
            ?? [],
            // Leftmost element of every row.
            NavigationDirection.Left => entitiesByRow
                .SelectMany(row => row.Where(e => e is INavigable && e.VisibleSelf).Take(1))
                .OfType<INavigable>(),
            // Rightmost element of every row.
            NavigationDirection.Right => entitiesByRow
                .SelectMany(row => row.Where(e => e is INavigable && e.VisibleSelf).TakeLast(1))
                .OfType<INavigable>(),
            // All elements of last row with stuff in it.
            NavigationDirection.Down => entitiesByRow
                .Where(row => row.Any(e => e is INavigable && e.VisibleSelf))
                .LastOrDefault()
                ?.OfType<INavigable>()
            ?? [],
            _ => throw new ArgumentException($"{direction}"),
        };

    private record GridCell(int Row, int Column)
    {
        internal GridCell Next(GridGroup parent) =>
            Column == parent.Columns - 1 ? new(Row + 1, 0) : new(Row, Column + 1);
    }

    private GridCell nextEmpty = new(0, 0);

    private bool IsFull(GridCell cell) =>
        cell.Row < Rows && entitiesByRow[cell.Row][cell.Column] != null;
}
