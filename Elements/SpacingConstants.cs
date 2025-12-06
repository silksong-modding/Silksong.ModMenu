using UnityEngine;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Standard spacing parameters for rows and columns.
/// </summary>
public static class SpacingConstants
{
    /// <summary>
    /// About 1/4 the screen's width, making it good for 4-column setups.
    /// </summary>
    public static float HSPACE_SMALL => 1000f / Camera.main.aspect;

    /// <summary>
    /// About 1/3 the screen's width, making it good for 3-column setups.
    /// </summary>
    public static float HSPACE_MEDIUM => 1333f / Camera.main.aspect;

    /// <summary>
    /// About 1/2 the screen's width, making it good for 2-column setups.
    /// </summary>
    public static float HSPACE_LARGE => 2000f / Camera.main.aspect;

    /// <summary>
    /// Spacing for about 12 small-font elements in a single column.
    /// </summary>
    public static float VSPACE_SMALL => 70f;

    /// <summary>
    /// Spacing for about 8 medium-font elements in a single column.
    /// </summary>
    public static float VSPACE_MEDIUM => 105f;

    /// <summary>
    /// Spacing for about 6 large-font elements in a single column.
    /// </summary>
    public static float VSPACE_LARGE => 160f;

    /// <summary>
    /// The RectTransform anchor position for the top center element of a standard menu.
    /// </summary>
    public static readonly Vector2 TOP_CENTER_ANCHOR = new(0, 330);
}
