using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// A component which can be used to scroll any child object into the center of,
/// and manages the smooth-scrolling coroutine for, the attached <see cref="ScrollRect"/>.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
internal class ScrollFocusController : UIBehaviour
{
    /// <summary>
    /// The time, in seconds, that it takes to smooth-scroll between transforms.
    /// </summary>
    public float smoothScrollTime = 0.2f;

    ScrollRect scrollRect;
    Coroutine? smoothScrollRoutine;
    bool menuAppearing = true;

    protected override void Awake()
    {
        base.Awake();
        scrollRect = GetComponent<ScrollRect>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        menuAppearing = true;
    }

    /// <summary>
    /// Used by <see cref="ScrollNavigationHelper"/> to instant-scroll a selectable into view
    /// ONLY if this is the first time the scroll pane is appearing.
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/silksong-modding/Silksong.ModMenu/pull/71#discussion_r3139673709">This Github comment</see>
    /// describes the problem that this is solving in more detail. In brief, selectables receive
    /// identical OnSelect events when a menu screen is appearing and when the mouse moves away from
    /// them without immediately touching another selectable, and we want different auto-focusing
    /// behaviour for these two cases.
    /// </remarks>
    internal void ScrollToIfOnMenuShow(Transform target)
    {
        if (menuAppearing)
        {
            ScrollTo(target, smooth: false);

            // Prevent unintended instant-focuses when a screen has multiple scroll panes
            var menu = GetComponentInParent<MenuScreen>();
            if (menu)
            {
                foreach (
                    var child in menu.transform.GetComponentsInChildren<ScrollFocusController>(true)
                )
                    child.SetMenuAppeared();
            }
        }
        menuAppearing = false;
    }

    internal void SetMenuAppeared() => menuAppearing = false;

    /// <summary>
    /// Scrolls this viewport and all scrolling viewports this one is nested within so that the
    /// entire hierarchy of viewports is centered on given <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The transform to scroll to.</param>
    /// <param name="smooth">
    ///     If false, the viewport position will change instantly.
    ///     If true, the target will move into view smoothly over a short time.
    /// </param>
    public void ScrollTo(Transform target, bool smooth = false)
    {
        foreach (var controller in GetComponentsInParent<ScrollFocusController>())
            controller.ScrollToInternal(target, smooth);
    }

    private void ScrollToInternal(Transform target, bool smooth)
    {
        menuAppearing = false;
        if (smoothScrollRoutine != null)
        {
            StopCoroutine(smoothScrollRoutine);
            smoothScrollRoutine = null;
        }

        if (smooth)
            smoothScrollRoutine = StartCoroutine(Coro());
        else
            scrollRect.normalizedPosition = GetScrollPoint(target.position);

        IEnumerator Coro()
        {
            Vector2 oldPos = scrollRect.normalizedPosition,
                newPos = GetScrollPoint(target.position);

            var curveX = AnimationCurve.EaseInOut(0, oldPos.x, smoothScrollTime, newPos.x);
            var curveY = AnimationCurve.EaseInOut(0, oldPos.y, smoothScrollTime, newPos.y);

            for (float time = 0; time <= smoothScrollTime; time += Time.deltaTime)
            {
                // Scroll point is re-evaluated each frame so that nested ScrollRects all
                // end at the appropriate scroll position for the target's final world position
                newPos = GetScrollPoint(target.position);

                curveX.SetKeys([curveX.keys[0], curveX.keys[^1] with { value = newPos.x }]);
                curveY.SetKeys([curveY.keys[0], curveY.keys[^1] with { value = newPos.y }]);

                scrollRect.normalizedPosition = new Vector2(
                    curveX.Evaluate(time),
                    curveY.Evaluate(time)
                );
                yield return null;
            }
            scrollRect.normalizedPosition = GetScrollPoint(target.position);
        }
    }

    /// <summary>
    /// The coordinates to set the scroll rect's normalized position to to scroll the
    /// given <paramref name="targetWorldPos"/> into the middle of the viewport.
    /// </summary>
    Vector2 GetScrollPoint(Vector2 targetWorldPos)
    {
        RectTransform viewport = scrollRect.viewport,
            content = scrollRect.content;

        Vector2 viewportSize = viewport.rect.size,
            contentSize = content.rect.size,
            contentSizeOffset = contentSize,
            targetPos = content.InverseTransformPoint(targetWorldPos);

        contentSizeOffset.Scale(content.pivot);
        contentSize.Scale(content.localScale);

        targetPos += contentSizeOffset;
        targetPos.Scale(content.localScale);

        Vector2 scrollPoint = Vector2.Clamp01(
            (targetPos - viewportSize * 0.5f) / (contentSize - viewportSize)
        );

        scrollPoint = scrollPoint with
        {
            x = scrollRect.horizontal ? scrollPoint.x : 0.5f,
            y = scrollRect.vertical ? scrollPoint.y : 0.5f,
        };

        return scrollPoint;
    }
}
