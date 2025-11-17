using Silksong.ModMenu.Internal;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Cardinal directions for navigating through the UI.
/// </summary>
public enum NavigationDirection
{
    Up,
    Left,
    Right,
    Down,
}

public static class NavigationDirectionExtensions
{
    public static NavigationDirection Opposte(this NavigationDirection self) =>
        self switch
        {
            NavigationDirection.Up => NavigationDirection.Down,
            NavigationDirection.Left => NavigationDirection.Right,
            NavigationDirection.Right => NavigationDirection.Left,
            NavigationDirection.Down => NavigationDirection.Up,
            _ => throw self.UnsupportedEnum(),
        };
}
