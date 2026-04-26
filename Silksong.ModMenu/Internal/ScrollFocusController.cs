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
    bool firstAppearance = true;

    protected override void Awake()
    {
        base.Awake();
        scrollRect = GetComponent<ScrollRect>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        firstAppearance = true;
    }

    /// <summary>
    /// Scrolls the viewport so it's centered on given <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The transform to scroll to.</param>
    /// <param name="smooth">
    ///     If false, the viewport position will change instantly.
    ///     If true, the target will move into view smoothly over a short time.
    /// </param>
    public void ScrollTo(Transform target, bool smooth = false)
    {
        firstAppearance = false;
        if (smoothScrollRoutine != null)
        {
            StopCoroutine(smoothScrollRoutine);
            smoothScrollRoutine = null;
        }

        Vector2 newPos = GetScrollPoint(target.position);
        newPos = newPos with
        {
            x = scrollRect.horizontal ? newPos.x : 0.5f,
            y = scrollRect.vertical ? newPos.y : 0.5f,
        };

        if (!smooth)
        {
            scrollRect.normalizedPosition = newPos;
            return;
        }

        smoothScrollRoutine = StartCoroutine(Coro());
        IEnumerator Coro()
        {
            Vector2 oldPos = scrollRect.normalizedPosition;
            var curveX = AnimationCurve.EaseInOut(0, oldPos.x, smoothScrollTime, newPos.x);
            var curveY = AnimationCurve.EaseInOut(0, oldPos.y, smoothScrollTime, newPos.y);

            for (float time = 0; time <= smoothScrollTime; time += Time.deltaTime)
            {
                scrollRect.normalizedPosition = new Vector2(
                    curveX.Evaluate(time),
                    curveY.Evaluate(time)
                );
                yield return null;
            }
            scrollRect.normalizedPosition = newPos;
        }
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
        if (firstAppearance)
            ScrollTo(target, smooth: false);
        firstAppearance = false;
    }

    /// <summary>
    /// The coordinates to set the scroll rect's normalized position to to scroll the
    /// given <paramref name="targetWorldPos"/> into the middle of the viewport.
    /// </summary>
    Vector2 GetScrollPoint(Vector2 targetWorldPos)
    {
        if (!scrollRect)
            return new Vector2(0.5f, 1);

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

        Vector2 scrollPoint = (targetPos - viewportSize * 0.5f) / (contentSize - viewportSize);

        return new(Mathf.Clamp01(scrollPoint.x), Mathf.Clamp01(scrollPoint.y));
    }
}
