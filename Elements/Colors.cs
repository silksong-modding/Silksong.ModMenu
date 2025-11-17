using UnityEngine;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Standard color scheme for menu elements. By default, changes to ElementState use these colors.
/// </summary>
public static class Colors
{
    /// <summary>
    /// Standard color of all menu text.
    /// </summary>
    public static readonly Color DEFAULT = Color.white;

    /// <summary>
    /// Color for menu text which is non-interactable.
    /// </summary>
    public static readonly Color DEFAULT_LOCKED = Color.grey;

    /// <summary>
    /// Bright color for a toggleable button that is set to 'true'.
    /// </summary>
    public static readonly Color TRUE = Color.Lerp(Color.white, Color.yellow, 0.5f);

    /// <summary>
    /// Semi-bright color for a toggleable button that is set to 'true' but non-interactable.
    /// </summary>
    public static readonly Color TRUE_LOCKED = Color.Lerp(Color.grey, Color.yellow, 0.5f);

    /// <summary>
    /// Dark color for a toggleable button that is set to 'false'.
    /// </summary>
    public static readonly Color FALSE = Color.grey;

    /// <summary>
    /// Dark color for a toggleable button that is set to 'false' and non-interactable.
    /// </summary>
    public static readonly Color FALSE_LOCKED = Color.Lerp(Color.grey, Color.black, 0.5f);

    /// <summary>
    /// Red color for an input with an invalid value.
    /// </summary>
    public static readonly Color INVALID = Color.Lerp(Color.white, Color.red, 0.5f);

    /// <summary>
    /// Dark red color for an input with an invalid value that is non-interactable.
    /// </summary>
    public static readonly Color INVALID_LOCKED = Color.Lerp(INVALID, Color.black, 0.5f);

    /// <summary>
    /// Get the default color for an element with the given state and interactability.
    /// </summary>
    public static Color GetDefaultColor(ElementState state, bool interactable)
    {
        return state switch
        {
            ElementState.TRUE => interactable ? TRUE : TRUE_LOCKED,
            ElementState.FALSE => interactable ? FALSE : FALSE_LOCKED,
            ElementState.INVALID => interactable ? FALSE : INVALID_LOCKED,
            _ => interactable ? DEFAULT : DEFAULT_LOCKED,
        };
    }

    /// <summary>
    /// Convenience getter for GetDefaultColor on a MenuElement.
    /// </summary>
    public static Color GetDefaultColor(MenuElement menuElement) =>
        GetDefaultColor(menuElement.State, false);

    /// <summary>
    /// Convenience getter for GetDefaultColor on a SelectableMenuElement.
    /// </summary>
    public static Color GetDefaultColor(SelectableElement selectableMenuElement) =>
        GetDefaultColor(selectableMenuElement.State, selectableMenuElement.Interactable);
}
