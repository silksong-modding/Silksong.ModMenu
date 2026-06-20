using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Helper functions for INavigable instances.
/// </summary>
public static class INavigableExtensions
{
    /// <summary>
    /// Helper functions for INavigable instances.
    /// </summary>
    extension(INavigable self)
    {
        /// <summary>
        /// Set the upwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborsUp(Selectable selectable) =>
            self.SetNeighbors(NavigationDirection.Up, [selectable]);

        /// <summary>
        /// Set the upwards neighbors of this navigable to one or more of the given options.
        /// </summary>
        public void SetNeighborsUp(IEnumerable<Selectable> selectables) =>
            self.SetNeighbors(NavigationDirection.Up, selectables);

        /// <summary>
        /// Set the upwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborsUp(SelectableElement selectableElement)
        {
            if (selectableElement.GetSelectables(NavigationDirection.Up, out var selectables))
                self.SetNeighborsUp(selectables);
        }

        /// <summary>
        /// Declare that this navigable has no upwards neighbors.
        /// </summary>
        public void ClearNeighborsUp() => self.ClearNeighbors(NavigationDirection.Up);

        /// <summary>
        /// Get the most eligible selectables within this navigable when navigating upwards into it.
        /// </summary>
        public bool GetNeighborsUp(
            [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
        ) => self.GetSelectables(NavigationDirection.Up, out selectables);

        /// <summary>
        /// Set the leftwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborsLeft(Selectable selectable) =>
            self.SetNeighbors(NavigationDirection.Left, [selectable]);

        /// <summary>
        /// Set the leftwards neighbors of this navigable to one or more of the given options.
        /// </summary>
        public void SetNeighborsLeft(IEnumerable<Selectable> selectables) =>
            self.SetNeighbors(NavigationDirection.Left, selectables);

        /// <summary>
        /// Set the leftwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborsLeft(SelectableElement selectableElement)
        {
            if (selectableElement.GetSelectables(NavigationDirection.Left, out var selectables))
                self.SetNeighborsLeft(selectables);
        }

        /// <summary>
        /// Declare that this navigable has no upwards neighbors.
        /// </summary>
        public void ClearNeighborsLeft() => self.ClearNeighbors(NavigationDirection.Left);

        /// <summary>
        /// Get the most eligible selectables within this navigable when navigating leftwards into it.
        /// </summary>
        public bool GetNeighborsLeft(
            [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
        ) => self.GetSelectables(NavigationDirection.Left, out selectables);

        /// <summary>
        /// Set the rightwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborsRight(Selectable selectable) =>
            self.SetNeighbors(NavigationDirection.Right, [selectable]);

        /// <summary>
        /// Set the rightwards neighbors of this navigable to one or more of the given options.
        /// </summary>
        public void SetNeighborsRight(IEnumerable<Selectable> selectables) =>
            self.SetNeighbors(NavigationDirection.Right, selectables);

        /// <summary>
        /// Set the rightwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborsRight(SelectableElement selectableElement)
        {
            if (selectableElement.GetSelectables(NavigationDirection.Right, out var selectables))
                self.SetNeighborsRight(selectables);
        }

        /// <summary>
        /// Get the most eligible selectables within this navigable when navigating rightwards into it.
        /// </summary>
        public bool GetNeighborsRight(
            [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
        ) => self.GetSelectables(NavigationDirection.Right, out selectables);

        /// <summary>
        /// Declare that this navigable has no upwards neighbors.
        /// </summary>
        public void ClearNeighborsRight() => self.ClearNeighbors(NavigationDirection.Right);

        /// <summary>
        /// Set the downwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborDown(Selectable selectable) =>
            self.SetNeighbors(NavigationDirection.Down, [selectable]);

        /// <summary>
        /// Set the downwards neighbors of this navigable to one or more of the given options.
        /// </summary>
        public void SetNeighborsDown(IEnumerable<Selectable> selectables) =>
            self.SetNeighbors(NavigationDirection.Down, selectables);

        /// <summary>
        /// Set the downwards neighbors of this navigable.
        /// </summary>
        public void SetNeighborsDown(SelectableElement selectableElement)
        {
            if (selectableElement.GetSelectables(NavigationDirection.Down, out var selectables))
                self.SetNeighborsDown(selectables);
        }

        /// <summary>
        /// Declare that this navigable has no upwards neighbors.
        /// </summary>
        public void ClearNeighborsDown() => self.ClearNeighbors(NavigationDirection.Down);

        /// <summary>
        /// Get the most eligible selectables within this navigable when navigating downwards into it.
        /// </summary>
        public bool GetNeighborsDown(
            [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
        ) => self.GetSelectables(NavigationDirection.Down, out selectables);

        /// <summary>
        /// Symmetrically connect two INavigables.
        /// </summary>
        public void ConnectSymmetric(INavigable dest, NavigationDirection direction)
        {
            if (dest.GetSelectables(direction, out var s))
                self.SetNeighbors(direction, s);
            if (self.GetSelectables(direction.Opposite(), out s))
                dest.SetNeighbors(direction.Opposite(), s);
        }
    }
}
