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
}
