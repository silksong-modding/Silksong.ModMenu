using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Component for improving the keyboard, controller, drag, and scrollwheel navigation
/// of <see cref="Selectable"/>s within a <see cref="ScrollRect"/>.
/// </summary>
internal class ScrollNavigationHelper : EventTrigger
{
    ScrollRect scrollRect;
    ScrollFocusController focusController;

    void Awake()
    {
        scrollRect = GetComponentInParent<ScrollRect>(true);
        focusController = GetComponentInParent<ScrollFocusController>(true);

        if (!scrollRect)
            throw new InvalidOperationException($"Failed to find containing {nameof(ScrollRect)}.");
        if (!focusController)
            throw new InvalidOperationException(
                $"Failed to find containing {nameof(ScrollFocusController)}."
            );
    }

    /// <summary>
    /// Automatically destroy this component if its object is re-parented, to
    /// ensure elements moved out of scroll panes stop affecting their scrolling.
    /// </summary>
    void OnTransformParentChanged() => Destroy(this);

    #region Intercepted UI Events

    /// <summary>
    /// When this selectable is selected through keyboard/controller navigation,
    /// or force-selected when the scroll pane first appears, the viewport will
    /// scroll so that this selectable is in its center.
    /// </summary>
    public override void OnSelect(BaseEventData eventData)
    {
        // When keyboard/controller navigated to.
        if (eventData is AxisEventData)
            focusController.ScrollTo(transform, smooth: true);
        // When force-selected. (e.x. when a menu is shown)
        // Can't avoid the type check or use `is` because then this would catch PointerEventData,
        // and instant-scrolling as a result of mouse movement is very jarring.
        else if (eventData.GetType() == typeof(BaseEventData))
            focusController.ScrollToIfOnMenuShow(transform);
    }

    /// <summary>
    /// Allows mouse-wheel scrolling when hovering over a selectable with this component;
    /// needed because EventTrigger components stop all events from bubbling.
    /// </summary>
    public override void OnScroll(PointerEventData eventData) => scrollRect.OnScroll(eventData);

    /// <summary>
    /// Allows click-and-drag scrolling when hovering over a selectable with this component;
    /// needed because EventTrigger components stop all events from bubbling.
    /// </summary>
    public override void OnDrag(PointerEventData eventData) => scrollRect.OnDrag(eventData);

    /// <inheritdoc cref="OnDrag"/>
    public override void OnBeginDrag(PointerEventData eventData) =>
        scrollRect.OnBeginDrag(eventData);

    /// <inheritdoc cref="OnDrag"/>
    public override void OnEndDrag(PointerEventData eventData) => scrollRect.OnEndDrag(eventData);

    /// <inheritdoc cref="OnDrag"/>
    public override void OnInitializePotentialDrag(PointerEventData eventData) =>
        scrollRect.OnInitializePotentialDrag(eventData);

    #endregion
}
