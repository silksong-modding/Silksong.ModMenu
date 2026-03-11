namespace Silksong.ModMenu.Screens;

/// <summary>
/// Whether to remember the user's last selectable when re-opening this menu.
/// </summary>
public enum SelectOnShowBehaviour
{
    /// <summary>
    /// Never remember the last selectable.
    /// </summary>
    AlwaysForget,

    /// <summary>
    /// Forget the last selectable when navigating backwards.
    /// </summary>
    ForgetBackwards,

    /// <summary>
    /// Always remember the last selectable.
    /// </summary>
    NeverForget,
}
