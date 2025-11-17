namespace Silksong.ModMenu.Screens;

/// <summary>
/// When showing a new menu screen, this enum indicates how the menu screen history should be modified.
/// </summary>
public enum HistoryMode
{
    /// <summary>
    /// Navigate forwards, adding this window to the end of the history stack.
    /// </summary>
    Add,

    /// <summary>
    /// Navigate sideways, replacing the previously shown window in the history stack.
    /// </summary>
    Replace,
}
