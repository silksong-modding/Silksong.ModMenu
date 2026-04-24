using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Component for improving the keyboard, controller, drag, and scrollwheel navigation
/// of <see cref="Selectable"/>s within a <see cref="UnityEngine.UI.ScrollRect"/>.
/// </summary>
internal class ScrollNavigationHelper : EventTrigger
{
    /// <summary>
    /// The ScrollRect that this selectable is inside of.
    /// </summary>
    public ScrollRect? ScrollRect { get; set; }

    static readonly Dictionary<ScrollRect, Coroutine> scrollRoutines = [];
    const float SMOOTH_SCROLL_TIME = 0.2f;

    static readonly PropertyInfo dummyEvent = typeof(EventSystem).GetProperty(
        "baseEventDataCache",
        BindingFlags.NonPublic | BindingFlags.Instance
    );

    public void ScrollToInstant()
    {
        CancelSmoothScroll();
        if (ScrollRect)
        {
            Vector2 pos = GetScrollPoint();
            ScrollRect.normalizedPosition = new Vector2(
                ScrollRect.horizontal ? pos.x : 0.5f,
                ScrollRect.vertical ? pos.y : 0.5f
            );
        }
    }

    public void ScrollToSmooth()
    {
        CancelSmoothScroll();
        if (ScrollRect)
            scrollRoutines[ScrollRect] = ScrollRect.StartCoroutine(Coro());

        IEnumerator Coro()
        {
            var scrollRect = ScrollRect;

            Vector2 pos = GetScrollPoint();
            pos = new Vector2(
                scrollRect.horizontal ? pos.x : 0.5f,
                scrollRect.vertical ? pos.y : 0.5f
            );

            var curveX = AnimationCurve.EaseInOut(
                0,
                scrollRect.normalizedPosition.x,
                SMOOTH_SCROLL_TIME,
                pos.x
            );
            var curveY = AnimationCurve.EaseInOut(
                0,
                scrollRect.normalizedPosition.y,
                SMOOTH_SCROLL_TIME,
                pos.y
            );

            for (float time = 0; time <= SMOOTH_SCROLL_TIME; time += Time.deltaTime)
            {
                scrollRect.normalizedPosition = new Vector2(
                    curveX.Evaluate(time),
                    curveY.Evaluate(time)
                );
                yield return null;
            }

            scrollRect.normalizedPosition = pos;
            scrollRoutines.Remove(scrollRect);
        }
    }

    #region Unity Messages

    /// <summary>
    /// Automatically destroy this component if its object is re-parented, to
    /// ensure elements moved out of scroll panes stop affecting their scrolling.
    /// </summary>
    void OnTransformParentChanged() => Destroy(this);

    /// <summary>
    /// When this selectable is selected through keyboard/controller navigation, the
    /// scroll pane will scroll so that it's centered in the viewport.
    /// It will also be scrolled to if it's being force-selected and is fully outside of the viewport.
    /// </summary>
    public override void OnSelect(BaseEventData eventData)
    {
        var rt = (RectTransform)transform;

        // when force-selected
        if (
            ReferenceEquals(eventData, dummyEvent.GetValue(EventSystem.current))
            && ScrollRect
            && !rt.Overlaps(ScrollRect.viewport)
        )
        {
            ScrollToInstant();
        }
        // when keyboard/controller navigated
        else if (eventData is AxisEventData)
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

    #endregion
    #region Utils

    /// <summary>
    /// The coordinates to set the scroll rect's normalized position to
    /// in order to scroll this selectable into the middle of the viewport.
    /// </summary>
    Vector2 GetScrollPoint()
    {
        if (!ScrollRect)
            return new Vector2(0.5f, 1);

        Vector2 viewportSize = ScrollRect.viewport.rect.size,
            contentScale = ScrollRect.content.localScale,
            contentSize = ScrollRect.content.rect.size,
            contentSizeOffset = contentSize,
            itemPos = ScrollRect.content.InverseTransformPoint(transform.position);

        contentSizeOffset.Scale(ScrollRect.content.pivot);
        contentSize.Scale(contentScale);

        itemPos += contentSizeOffset;
        itemPos.Scale(contentScale);

        Vector2 scrollPoint = (itemPos - viewportSize * 0.5f) / (contentSize - viewportSize);

        return new(Mathf.Clamp01(scrollPoint.x), Mathf.Clamp01(scrollPoint.y));
    }

    bool CanInstantScroll() =>
        ScrollRect
        && !scrollRoutines.ContainsKey(ScrollRect)
        && ScrollRect.content.sizeDelta.y > ScrollRect.viewport.sizeDelta.y;

    void CancelSmoothScroll()
    {
        if (ScrollRect && scrollRoutines.TryGetValue(ScrollRect, out var coro))
        {
            ScrollRect.StopCoroutine(coro);
            scrollRoutines.Remove(ScrollRect);
        }
    }

    #endregion
}
