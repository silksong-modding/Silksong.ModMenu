using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

[RequireComponent(typeof(MenuOptionHorizontal))]
internal class CustomMenuOptionHorizontal : MonoBehaviour
{
    private MenuOptionHorizontal? orig;

    private void Awake()
    {
        orig = GetComponent<MenuOptionHorizontal>();
        UpdateText();
    }

    internal IBaseChoiceModel? Model;

    internal void UpdateText()
    {
        if (orig == null)
            return;

        orig.optionText.text = Model?.DisplayString() ?? "???";
        if (orig.optionText.TryGetComponent<FixVerticalAlign>(out var align))
            align.AlignText();
    }

    private static bool DoOrig(MenuOptionHorizontal self, out CustomMenuOptionHorizontal custom)
    {
        custom = self.gameObject.GetComponent<CustomMenuOptionHorizontal>();
        return custom == null;
    }

    private static bool OverrideMoveOption(
        On.UnityEngine.UI.MenuOptionHorizontal.orig_MoveOption orig,
        MenuOptionHorizontal self,
        MoveDirection dir
    )
    {
        if (DoOrig(self, out var custom))
            return orig(self, dir);
        if (custom.Model == null)
            return false;

        switch (dir)
        {
            case MoveDirection.Right:
                if (!custom.Model.MoveRight())
                    return false;
                break;
            case MoveDirection.Left:
                if (!custom.Model.MoveLeft())
                    return false;
                break;
            default:
                return false;
        }

        self.PlaySlider();
        return true;
    }

    private static void OverrideUpdateText(
        On.UnityEngine.UI.MenuOptionHorizontal.orig_UpdateText orig,
        MenuOptionHorizontal self
    )
    {
        if (DoOrig(self, out var custom))
        {
            orig(self);
            return;
        }

        custom.UpdateText();
    }

    static CustomMenuOptionHorizontal()
    {
        On.UnityEngine.UI.MenuOptionHorizontal.MoveOption += OverrideMoveOption;
        On.UnityEngine.UI.MenuOptionHorizontal.UpdateText += OverrideUpdateText;
    }
}
