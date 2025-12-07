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
public class GridGroup(int columns) : AbstractGroup
{
    private readonly List<IMenuEntity?[]> rows = [];
    private readonly Dictionary<IMenuEntity, GridCell> index = [];

    /// <summary>
    /// The number of columns in this grid.
    /// </summary>
    public readonly int Columns =
        columns > 0 ? columns : throw new ArgumentException($"Columns: {columns}");

    /// <summary>
    /// The number of rows in this grid.
    /// </summary>
    public int Rows => rows.Count;

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

    /// <summary>
    /// Add this entity to the next available empty cell in the grid.
    /// </summary>
    public void Add(IMenuEntity entity)
    {
        while (IsFull(nextEmptyCell))
            nextEmptyCell = nextEmptyCell.Next(this);

        AddAt(nextEmptyCell.Row, nextEmptyCell.Column, entity);
        nextEmptyCell = nextEmptyCell.Next(this);
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

        GridCell cell = new(row, column);
        if (index.TryGetValue(entity, out var prevCell))
        {
            if (cell == prevCell)
                return; // Nothing to do.
            else
                throw new ArgumentException($"Entity already present at ({row}, {column})");
        }

        if (IsFull(cell))
            throw new ArgumentException($"({row}, {column}) is already filled.");

        while (rows.Count <= row)
            rows.Add(new IMenuEntity?[Columns]);
        rows[row][column] = entity;
        index[entity] = cell;
        AddChild(entity);
    }

    /// <summary>
    /// Remove the specified entity from the grid.
    /// </summary>
    public bool Remove(IMenuEntity entity)
    {
        if (!index.TryGetValue(entity, out var cell))
            return false;

        index.Remove(entity);
        rows[cell.Row][cell.Column] = null;
        nextEmptyCell = cell.CompareTo(nextEmptyCell) <= 0 ? cell : nextEmptyCell;
        entity.ClearParents();

        // Truncate empty rows.
        if (cell.Row == rows.Count - 1)
            for (int row = cell.Row; row >= 0 && rows[row].All(e => e == null); row--)
                rows.RemoveAt(row);
        return true;
    }

    /// <summary>
    /// Remove the entity at the specified cell.
    /// </summary>
    public bool RemoveAt(int row, int column) =>
        TryGetValue(new(row, column), out var entity) && Remove(entity);

    /// <inheritdoc/>
    public override bool GetSelectable(
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

    /// <inheritdoc/>
    public override void UpdateLayout(Vector2 localAnchorPos)
    {
        ClearNeighbors();

        // Update positions.
        foreach (var e in index)
        {
            var (entity, cell) = (e.Key, e.Value);

            Vector2 pos = localAnchorPos;
            pos.y -= VerticalSpacing * cell.Row;
            pos.x += HorizontalSpacing * (cell.Column - (Columns - 1) / 2f);
            entity.UpdateLayout(pos);
        }

        // Update navigation.
        INavigable?[]? prevRow = null;
        foreach (var row in rows)
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
            foreach (
                var (left, right) in (
                    WrapHorizontal
                        ? nextRow.WhereNonNull().CircularPairs()
                        : nextRow.WhereNonNull().Pairs()
                )
            )
                left.ConnectSymmetric(right, NavigationDirection.Right);
        }
    }

    /// <inheritdoc/>
    protected override IEnumerable<IMenuEntity> AllEntities() =>
        rows.SelectMany(row => row.WhereNonNull());

    private ListView<ListView<IMenuEntity?>> GetColumns() =>
        new(column => new(row => rows[row][column], rows.Count), Columns);

    /// <inheritdoc/>
    protected override IEnumerable<INavigable> GetNavigables(NavigationDirection direction) =>
        direction switch
        {
            // All elements of first row with stuff in it.
            NavigationDirection.Up => rows.Where(row =>
                    row.Any(e => e is INavigable && e.VisibleSelf)
                )
                .FirstOrDefault()
                ?.OfType<INavigable>()
                ?? [],
            // Leftmost element of every row.
            NavigationDirection.Left => rows.SelectMany(row =>
                    row.Where(e => e is INavigable && e.VisibleSelf).Take(1)
                )
                .OfType<INavigable>(),
            // Rightmost element of every row.
            NavigationDirection.Right => rows.SelectMany(row =>
                    row.Where(e => e is INavigable && e.VisibleSelf).TakeLast(1)
                )
                .OfType<INavigable>(),
            // All elements of last row with stuff in it.
            NavigationDirection.Down => rows.Where(row =>
                    row.Any(e => e is INavigable && e.VisibleSelf)
                )
                .LastOrDefault()
                ?.OfType<INavigable>()
                ?? [],
            _ => throw new ArgumentException($"{direction}"),
        };

    private record GridCell(int Row, int Column) : IComparable<GridCell>
    {
        internal GridCell Next(GridGroup parent) =>
            Column == parent.Columns - 1 ? new(Row + 1, 0) : new(Row, Column + 1);

        public int CompareTo(GridCell other) =>
            Row == other.Row ? Column.CompareTo(other.Column) : Row.CompareTo(other.Row);
    }

    private GridCell nextEmptyCell = new(0, 0);

    private bool TryGetValue(GridCell cell, [MaybeNullWhen(false)] out IMenuEntity entity)
    {
        entity = default;
        if (cell.Row < 0 || cell.Row >= rows.Count || cell.Column < 0 || cell.Column >= Columns)
            return false;

        entity = rows[cell.Row][cell.Column];
        return entity != null;
    }

    private bool IsFull(GridCell cell) => TryGetValue(cell, out _);
}
