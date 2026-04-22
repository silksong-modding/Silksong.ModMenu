using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Helper component for improving keyboard, controller, and scrollwheel navigation
/// on <see cref="Screens.ScrollingMenuScreen"/>s.
/// </summary>
internal class ScrollNavigationHelper : EventTrigger
{
    /// <summary>
    /// The ScrollRect that this selectable is part of.
    /// </summary>
    public ScrollRect? ScrollRect { get; set; }

    static Coroutine? scrollRoutine;
    const float SMOOTH_SCROLL_TIME = 0.2f;

    public void ScrollToInstant()
    {
        CancelSmoothScroll();
        if (ScrollRect)
            ScrollRect.normalizedPosition = new Vector2(0, GetScrollPoint());
    }

    public void ScrollToSmooth()
    {
        CancelSmoothScroll();
        if (ScrollRect)
            scrollRoutine = UIManager.instance.StartCoroutine(Coro());

        IEnumerator Coro()
        {
            float y = GetScrollPoint();
            var curve = AnimationCurve.EaseInOut(
                0,
                ScrollRect.normalizedPosition.y,
                SMOOTH_SCROLL_TIME,
                y
            );
            for (float time = 0; time <= SMOOTH_SCROLL_TIME; time += Time.deltaTime)
            {
                ScrollRect.normalizedPosition = new Vector2(0, curve.Evaluate(time));
                yield return null;
            }
            ScrollRect.normalizedPosition = new Vector2(0, y);
        }
    }

    /// <summary>
    /// When this selectable is selected through keyboard/controller navigation, the
    /// scroll pane will scroll so that it's centered in the viewport.
    /// </summary>
    public override void OnSelect(BaseEventData eventData)
    {
        if (eventData is not AxisEventData)
            return;

        if (ScrollRect && ScrollRect.content.sizeDelta.y > ScrollRect.viewport.sizeDelta.y)
        {
            ScrollToSmooth();
        }
    }

    /// <summary>
    /// Allows mouse-wheel scrolling when hovering over a selectable with this component;
    /// needed because EventTrigger components stop all events from bubbling.
    /// </summary>
    public override void OnScroll(PointerEventData eventData)
    {
        if (CanInstantScroll())
            ScrollRect!.OnScroll(eventData);
    }

    /// <summary>
    /// Allows click-and-drag scrolling when hovering over a selectable with this component;
    /// needed because EventTrigger components stop all events from bubbling.
    /// </summary>
    public override void OnDrag(PointerEventData eventData)
    {
        if (CanInstantScroll())
            ScrollRect!.OnDrag(eventData);
    }

    /// <inheritdoc cref="OnDrag"/>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (CanInstantScroll())
            ScrollRect!.OnBeginDrag(eventData);
    }

    /// <inheritdoc cref="OnDrag"/>
    public override void OnEndDrag(PointerEventData eventData)
    {
        if (CanInstantScroll())
            ScrollRect!.OnEndDrag(eventData);
    }

    /// <inheritdoc cref="OnDrag"/>
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (CanInstantScroll())
            ScrollRect!.OnInitializePotentialDrag(eventData);
    }

    void OnDestroy() => CancelSmoothScroll();

    /// <summary>
    /// The y-coordinate to set the scroll rect's normalized position to
    /// in order to scroll this selectable into the middle of the viewport.
    /// </summary>
    float GetScrollPoint()
    {
        if (!ScrollRect)
            return 1;

        Vector2 viewportSize = ScrollRect.viewport.rect.size,
            contentScale = ScrollRect.content.localScale,
            contentSize = ScrollRect.content.rect.size,
            contentSizeOffset = contentSize,
            itemPos = ScrollRect.content.InverseTransformPoint(gameObject.transform.position);

        contentSizeOffset.Scale(ScrollRect.content.pivot);
        contentSize.Scale(contentScale);

        itemPos += contentSizeOffset;
        itemPos.Scale(contentScale);

        return Mathf.Clamp01(
            (itemPos.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y)
        );
    }

    bool CanInstantScroll() =>
        ScrollRect
        && scrollRoutine == null
        && ScrollRect.content.sizeDelta.y > ScrollRect.viewport.sizeDelta.y;

    static void CancelSmoothScroll()
    {
        if (scrollRoutine != null)
        {
            UIManager.instance.StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
    }
}
