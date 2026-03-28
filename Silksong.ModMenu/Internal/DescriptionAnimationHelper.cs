using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal class DescriptionAnimationHelper
    : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        ICancelHandler
{
    private Selectable _selectable;
    public Animator? DescriptionAnimator { get; set; }

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
        if (DescriptionAnimator == null)
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
            DescriptionAnimator.ResetTrigger(MenuSelectable._showPropId);
            DescriptionAnimator.SetTrigger(MenuSelectable._hidePropId);
        }
        else if (prevSelectedObject != null && prevSelectedObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(prevSelectedObject);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (DescriptionAnimator == null)
        {
            return;
        }

        if (!_selectable.interactable)
        {
            return;
        }

        DescriptionAnimator.ResetTrigger(MenuSelectable._hidePropId);
        DescriptionAnimator.SetTrigger(MenuSelectable._showPropId);
    }

    public void OnCancel(BaseEventData eventData)
    {
        StartCoroutine(ValidateDeselect(eventData, true));
    }
}
