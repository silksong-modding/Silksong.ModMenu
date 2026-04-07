using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.HookGen;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Component to animate objects (such as a description and fleurs) on an element which is not a MenuSelectable,
/// when the element is selected.
/// </summary>
/// <remarks>
/// This component should not be added multiple times; instead, multiple animators should be added to
/// the <see cref="Animators"/> list of a single MenuSelectableAnimationProxy component.
/// </remarks>
[MonoDetourTargets(typeof(MenuSelectable))]
internal class MenuSelectableAnimationProxy
    : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        ICancelHandler
{
    private Selectable _selectable;
    private bool _isMenuSelectable;

    public List<Animator> Animators = [];

    void Awake()
    {
        _selectable = GetComponent<Selectable>();
        // If the element is already a menu selectable, then the animation will break
        // if the usual method is used, so we delegate to the IL hook.
        _isMenuSelectable = _selectable is MenuSelectable;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_isMenuSelectable)
        {
            return;
        }
        StartCoroutine(ValidateDeselect(eventData, false));
    }

    // My best attempt to imitate the code from MenuSelectable.ValidateDeselect
    private IEnumerator ValidateDeselect(BaseEventData eventData, bool force)
    {
        if (Animators.Count == 0)
        {
            yield break;
        }

        GameObject? prevSelectedObject = EventSystem.current.currentSelectedGameObject;

        yield return new WaitForEndOfFrame();

        if (!(EventSystem.current.currentSelectedGameObject != null || force))
        {
            InputHandler ih = ManagerSingleton<InputHandler>.Instance;
            if (ih && !ih.acceptingInput)
            {
                while (!ih.acceptingInput)
                {
                    yield return null;
                }
            }
            yield return null;
        }

        if (EventSystem.current.currentSelectedGameObject != null || force)
        {
            foreach (Animator animator in Animators)
            {
                animator.ResetTrigger(MenuSelectable._showPropId);
                animator.SetTrigger(MenuSelectable._hidePropId);
            }
        }
        else if (prevSelectedObject != null && prevSelectedObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(prevSelectedObject);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_isMenuSelectable)
        {
            return;
        }

        if (Animators.Count == 0)
        {
            return;
        }

        if (!_selectable.interactable)
        {
            return;
        }

        foreach (Animator animator in Animators)
        {
            animator.ResetTrigger(MenuSelectable._hidePropId);
            animator.SetTrigger(MenuSelectable._showPropId);
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        StartCoroutine(ValidateDeselect(eventData, true));
    }

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
                // this.descriptionText.SetTrigger(MenuSelectable._showPropId)
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
                // this.descriptionText.SetTrigger(MenuSelectable._hidePropId)
                // It is Ldloc rather than Ldarg.0 because it is the IEnumerator.MoveNext method
                // that is being hooked
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
        MenuSelectableAnimationProxy proxy =
            selectable.gameObject.GetComponent<MenuSelectableAnimationProxy>();
        if (proxy != null)
        {
            foreach (Animator anim in proxy.Animators)
            {
                anim.ResetTrigger(MenuSelectable._hidePropId);
                anim.SetTrigger(MenuSelectable._showPropId);
            }
        }
    }

    private static void AnimateDown(MenuSelectable selectable)
    {
        MenuSelectableAnimationProxy proxy =
            selectable.gameObject.GetComponent<MenuSelectableAnimationProxy>();
        if (proxy != null)
        {
            foreach (Animator anim in proxy.Animators)
            {
                anim.ResetTrigger(MenuSelectable._showPropId);
                anim.SetTrigger(MenuSelectable._hidePropId);
            }
        }
    }
}
