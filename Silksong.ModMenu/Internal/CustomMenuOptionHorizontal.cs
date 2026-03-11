using MonoDetour;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

[MonoDetourTargets(typeof(MenuOptionHorizontal), GenerateControlFlowVariants = true)]
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

        orig.optionText.LocalizedText = Model?.DisplayString() ?? "???";
        if (orig.optionText.TryGetComponent<FixVerticalAlign>(out var align))
            align.AlignText();
    }

    private bool MoveOption(MoveDirection dir)
    {
        if (Model == null)
            return false;

        switch (dir)
        {
            case MoveDirection.Right:
                if (!Model.MoveRight())
                    return false;
                break;
            case MoveDirection.Left:
                if (!Model.MoveLeft())
                    return false;
                break;
            default:
                return false;
        }

        if (orig != null && orig.uiAudioPlayer != null)
            orig.uiAudioPlayer.PlaySlider();
        return true;
    }

    private static ReturnFlow OverrideMoveOption(
        MenuOptionHorizontal self,
        ref MoveDirection dir,
        ref bool returnValue
    )
    {
        if (!self.TryGetComponent<CustomMenuOptionHorizontal>(out var custom))
            return ReturnFlow.None;

        returnValue = custom.MoveOption(dir);
        return ReturnFlow.SkipOriginal;
    }

    private static ReturnFlow OverrideUpdateText(MenuOptionHorizontal self)
    {
        if (!self.TryGetComponent<CustomMenuOptionHorizontal>(out var custom))
            return ReturnFlow.None;

        custom.UpdateText();
        return ReturnFlow.SkipOriginal;
    }

    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.UnityEngine.UI.MenuOptionHorizontal.MoveOption.ControlFlowPrefix(OverrideMoveOption);
        Md.UnityEngine.UI.MenuOptionHorizontal.UpdateText.ControlFlowPrefix(OverrideUpdateText);
    }
}
