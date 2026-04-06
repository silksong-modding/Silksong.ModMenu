using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal class MenuSelectableAnimationProxy
    : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        ICancelHandler
{
    private Selectable _selectable;
    public List<Animator> Animators = [];

    void Awake()
    {
        _selectable = GetComponent<Selectable>();
    }

    public void OnDeselect(BaseEventData eventData)
    {
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
}
