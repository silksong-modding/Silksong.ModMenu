namespace Silksong.ModMenu.Elements;

/// <summary>
/// Semantic states for an interactible menu element, which control colors.
/// </summary>
public enum ElementState
{
    /// <summary>
    /// Element with no real state.
    /// </summary>
    DEFAULT,

    /// <summary>
    /// Element that can be true or false, but is true.
    /// </summary>
    TRUE,

    /// <summary>
    /// Element that can be true or false, but is false.
    /// </summary>
    FALSE,

    /// <summary>
    /// Element with an invalid setting that must be corrected.
    /// </summary>
    INVALID,
}
