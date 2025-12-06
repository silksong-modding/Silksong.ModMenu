using System;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Helper functions for working with navigation directions.
/// </summary>
public static class NavigationDirectionExtensions
{
    /// <summary>
    /// Get the opposite direction of this one.
    /// </summary>
    public static NavigationDirection Opposite(this NavigationDirection self) =>
        self switch
        {
            NavigationDirection.Up => NavigationDirection.Down,
            NavigationDirection.Left => NavigationDirection.Right,
            NavigationDirection.Right => NavigationDirection.Left,
            NavigationDirection.Down => NavigationDirection.Up,
            _ => throw new ArgumentException($"{self}"),
        };

    /// <summary>
    /// Symmetrically connect two INavigables.
    /// </summary>
    public static void ConnectPair(this NavigationDirection self, INavigable src, INavigable dst)
    {
        if (dst.GetSelectable(self, out var s))
            src.SetNeighbor(self, s);
        if (src.GetSelectable(self.Opposite(), out s))
            dst.SetNeighbor(self.Opposite(), s);
    }
}
