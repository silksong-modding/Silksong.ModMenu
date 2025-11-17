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
        /// Set the upwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborUp(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Up, selectable);

        /// <summary>
        /// Set the upwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborUp(SelectableElement selectableElement) =>
            self.SetNeighborUp(selectableElement.SelectableComponent);

        /// <summary>
        /// Declare that this navigable has no upwards neighbor.
        /// </summary>
        public void ClearNeighborUp() => self.ClearNeighbor(NavigationDirection.Up);

        /// <summary>
        /// Get the most eligible selectable within this navigable when navigating upwards into it.
        /// </summary>
        public bool GetNeighborUp([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Up, out selectable);

        /// <summary>
        /// Set the leftwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborLeft(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Left, selectable);

        /// <summary>
        /// Set the leftwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborLeft(SelectableElement selectableElement) =>
            self.SetNeighborLeft(selectableElement.SelectableComponent);

        /// <summary>
        /// Declare that this navigable has no upwards neighbor.
        /// </summary>
        public void ClearNeighborLeft() => self.ClearNeighbor(NavigationDirection.Left);

        /// <summary>
        /// Get the most eligible selectable within this navigable when navigating leftwards into it.
        /// </summary>
        public bool GetNeighborLeft([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Left, out selectable);

        /// <summary>
        /// Set the rightwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborRight(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Right, selectable);

        /// <summary>
        /// Set the rightwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborRight(SelectableElement selectableElement) =>
            self.SetNeighborRight(selectableElement.SelectableComponent);

        /// <summary>
        /// Get the most eligible selectable within this navigable when navigating rightwards into it.
        /// </summary>
        public bool GetNeighborRight([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Right, out selectable);

        /// <summary>
        /// Declare that this navigable has no upwards neighbor.
        /// </summary>
        public void ClearNeighborRight() => self.ClearNeighbor(NavigationDirection.Right);

        /// <summary>
        /// Set the downwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborDown(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Down, selectable);

        /// <summary>
        /// Set the downwards neighbor of this navigable.
        /// </summary>
        public void SetNeighborDown(SelectableElement selectableElement) =>
            self.SetNeighborDown(selectableElement.SelectableComponent);

        /// <summary>
        /// Declare that this navigable has no upwards neighbor.
        /// </summary>
        public void ClearNeighborDown() => self.ClearNeighbor(NavigationDirection.Down);

        /// <summary>
        /// Get the most eligible selectable within this navigable when navigating downwards into it.
        /// </summary>
        public bool GetNeighborDown([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Down, out selectable);
    }
}
