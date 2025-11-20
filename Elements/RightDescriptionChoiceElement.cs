using System;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.HookGen;
using MonoMod.Cil;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Subclass of <see cref="ChoiceElement{T}"/> with a second description object that lies below the choice text.
/// </summary>
public class RightDescriptionChoiceElement<T> : ChoiceElement<T>
{
    internal const string RIGHT_DESCRIPTION_NAME = "ModMenu-Right Description";

    /// <summary>
    /// The Unity component for the text object added on the right hand side (below the <see cref="ChoiceElement{T}.ChoiceText"/>).
    /// </summary>
    public readonly Text RightText;

    /// <inheritdoc cref="ChoiceElement{T}.ChoiceElement(string, IChoiceModel{T}, string)"/>
    public RightDescriptionChoiceElement(
        string label,
        IChoiceModel<T> model,
        string description,
        string rightDescription
    )
        : base(label, model, description)
    {
        RightText = SetupRightDescription();
        RightText.text = rightDescription;
    }

    /// <summary>
    /// Create a choice element with a description below the choice that updates based on the value of the underlying Model.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="model"></param>
    /// <param name="description"></param>
    /// <param name="getRightDescription">Function used to determine the description below the choice.</param>
    public RightDescriptionChoiceElement(
        string label,
        IChoiceModel<T> model,
        string description,
        Func<T, string> getRightDescription
    )
        : this(label, model, description, getRightDescription(model.Value))
    {
        model.OnValueChanged += t => RightText.text = getRightDescription(model.Value);
    }

    private Text SetupRightDescription()
    {
        GameObject desc = DescriptionText.gameObject;
        GameObject rightDesc = UObject.Instantiate(
            desc,
            desc.transform.parent,
            worldPositionStays: true
        );
        rightDesc.name = RIGHT_DESCRIPTION_NAME;
        rightDesc.SetActive(true);
        Text rightText = rightDesc.GetComponent<Text>();
        rightText.alignment = TextAnchor.MiddleRight;

        RectTransform originalRect = desc.GetComponent<RectTransform>();
        RectTransform newRect = rightDesc.GetComponent<RectTransform>();
        RectTransform optionRect = ChoiceText.gameObject.GetComponent<RectTransform>();

        newRect.anchorMin = new(1f, 0.5f);
        newRect.anchorMax = new(1f, 0.5f);
        newRect.pivot = new(1f, 0.5f);
        newRect.anchoredPosition = new(
            optionRect.anchoredPosition.x + (0.5f * optionRect.sizeDelta.x), // TODO - fix this
            originalRect.anchoredPosition.y
        );

        rightDesc.GetComponent<ChangePositionByLanguage>().originalPosition = rightDesc
            .transform
            .localPosition;

        return rightText;
    }
}

// Separate class because this can't be done on a generic class
[MonoDetourTargets(typeof(MenuSelectable))]
internal static class RightDescriptionChoiceElementHooks
{
    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.UnityEngine.UI.MenuSelectable.OnSelect.ILHook(HookOnSelect);
        Md.UnityEngine.UI.MenuSelectable.ValidateDeselect.ILHookMoveNext(HookOnDeselect);
    }

    private static void HookOnSelect(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        while (
            cursor.TryGotoNext(
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<MenuSelectable>(nameof(MenuSelectable.descriptionText)),
                i => i.MatchLdsfld<MenuSelectable>(nameof(MenuSelectable._showPropId)),
                i => i.MatchCallOrCallvirt<Animator>(nameof(Animator.SetTrigger))
            )
        )
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(AnimateUp);
        }
    }

    private static void HookOnDeselect(ILManipulationInfo info)
    {
        ILCursor cursor = new(info.Context);
        int locIndex = 1;
        while (
            cursor.TryGotoNext(
                MoveType.After,
                i => i.MatchLdloc(out locIndex),
                i => i.MatchLdfld<MenuSelectable>(nameof(MenuSelectable.descriptionText)),
                i => i.MatchLdsfld<MenuSelectable>(nameof(MenuSelectable._hidePropId)),
                i => i.MatchCallOrCallvirt<Animator>(nameof(Animator.SetTrigger))
            )
        )
        {
            cursor.Emit(OpCodes.Ldloc, locIndex);
            cursor.EmitDelegate(AnimateDown);
        }
    }

    private static void AnimateUp(MenuSelectable selectable)
    {
        GameObject? rightDesc = selectable.gameObject.FindChild(
            RightDescriptionChoiceElement<object>.RIGHT_DESCRIPTION_NAME
        );

        if (rightDesc != null)
        {
            Animator anim = rightDesc.GetComponent<Animator>();
            anim.ResetTrigger(MenuSelectable._hidePropId);
            anim.SetTrigger(MenuSelectable._showPropId);
        }
    }

    private static void AnimateDown(MenuSelectable selectable)
    {
        GameObject? rightDesc = selectable.gameObject.FindChild(
            RightDescriptionChoiceElement<object>.RIGHT_DESCRIPTION_NAME
        );

        if (rightDesc != null)
        {
            Animator anim = rightDesc.GetComponent<Animator>();
            anim.ResetTrigger(MenuSelectable._showPropId);
            anim.SetTrigger(MenuSelectable._hidePropId);
        }
    }
}
