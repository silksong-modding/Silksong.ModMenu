using UnityEngine;

namespace Silksong.ModMenu.Elements;

public static class Colors
{
    public static readonly Color DEFAULT = Color.white;
    public static readonly Color DEFAULT_LOCKED = Color.grey;
    public static readonly Color TRUE = Color.Lerp(Color.white, Color.yellow, 0.5f);
    public static readonly Color TRUE_LOCKED = Color.Lerp(Color.grey, Color.yellow, 0.5f);
    public static readonly Color FALSE = Color.grey;
    public static readonly Color FALSE_LOCKED = Color.Lerp(Color.grey, Color.black, 0.5f);
    public static readonly Color INVALID = Color.Lerp(Color.white, Color.red, 0.5f);
    public static readonly Color INVALID_LOCKED = Color.Lerp(INVALID, Color.black, 0.5f);

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

    public static Color GetDefaultColor(MenuElement menuElement) =>
        GetDefaultColor(menuElement.State, false);

    public static Color GetDefaultColor(SelectableElement selectableMenuElement) =>
        GetDefaultColor(selectableMenuElement.State, selectableMenuElement.Interactable);
}
