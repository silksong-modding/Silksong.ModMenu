using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A scrolling panel that can contain an arbitrary amount of content.
/// </summary>
public class ScrollingPane : MenuDisposable, INavigableMenuEntity
{
    #region API

    /// <inheritdoc/>
    public VisibilityManager Visibility { get; } = new(false);

    /// <summary>
    /// The scrolling pane's content.
    /// </summary>
    public INavigableMenuEntity? Content
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field?.ClearParents();
            field = value;
            field?.SetParents(this, contentPane);
        }
    }

    /// <summary>
    /// The <see cref="ScrollRect"/> component that controls the
    /// scrolling sensitivity and movement type.
    /// </summary>
    public readonly ScrollRect scrollRect;

    /// <summary>
    /// Construct a new scrolling pane that displays the contents of <paramref name="content"/>.
    /// </summary>
    /// <param name="content"><inheritdoc cref="Content" path="//summary"/></param>
    public ScrollingPane(INavigableMenuEntity content)
        : base()
    {
        scrollPane = MenuPrefabs
            .Get()
            .NewScrollPane(out scrollRect, out focusController, out contentPane);

        Content = content;

        contentPane.AddComponent<OnChildTransformsChangeHelper>().OnChildrenChanged +=
            QueueContentResize;

        AddScrollNavHelpers();

        OnDispose += () => UObject.Destroy(scrollPane);
        Visibility.OnVisibilityChanged += visibleInHierarchy =>
            scrollPane.SetActive(visibleInHierarchy);

        scrollPane.SetActive(false);
    }

    /// <summary>
    /// The size of the visible area of the scroll pane.
    /// </summary>
    public Vector2 ViewportSize
    {
        get => scrollPane.RectTransform.sizeDelta;
        set => scrollPane.RectTransform.sizeDelta = value;
    }

    /// <summary>
    /// Whether this pane scrolls only vertically, only horizontally, or in both axes.
    /// </summary>
    public ScrollAxes Axes
    {
        get =>
            (scrollRect.vertical ? ScrollAxes.Vertical : 0)
            | (scrollRect.horizontal ? ScrollAxes.Horizontal : 0);
        set
        {
            scrollRect.vertical = value.HasFlag(ScrollAxes.Vertical);
            scrollRect.horizontal = value.HasFlag(ScrollAxes.Horizontal);
        }
    }

    /// <summary>
    /// Semantic states for which axes a <see cref="ScrollingPane"/> can scroll in.
    /// </summary>
    [Flags]
    public enum ScrollAxes
    {
        /// <summary>
        /// Exclusively vertical scrolling.
        /// </summary>
        Vertical = 0x01,

        /// <summary>
        /// Exclusively horizontal scrolling.
        /// </summary>
        Horizontal = 0x10,

        /// <summary>
        /// Scrolling in both the vertical and horizontal axes.
        /// </summary>
        Both = Vertical | Horizontal,
    }

    /// <summary>
    /// The time, in seconds, that it takes to smooth-scroll between menu elements.
    /// </summary>
    public float SmoothScrollTime
    {
        get => focusController.smoothScrollTime;
        set => focusController.smoothScrollTime = value;
    }

    /// <summary>
    /// Scrolls the viewport so it's centered on the given element,
    /// provided the element is in this group.
    /// </summary>
    /// <param name="element">The element to scroll to.</param>
    /// <param name="smooth">
    ///     If false, the viewport position will change instantly.
    ///     If true, the element will move into view smoothly over a short time.
    /// </param>
    public void ScrollTo(MenuElement element, bool smooth = false)
    {
        if (element.VisibleInHierarchy)
            focusController.ScrollTo(element.Container.transform, smooth);
    }

    /// <summary>
    /// Scrolls the viewport so it's centered on the element at the given index.
    /// </summary>
    /// <param name="index">The index of the element to scroll to.</param>
    /// <param name="smooth">
    ///     If false, the viewport position will change instantly.
    ///     If true, the element will move into view smoothly over a short time.
    /// </param>
    public void ScrollTo(int index, bool smooth = false)
    {
        var element = Content?.AllElements().ElementAtOrDefault(index);
        if (element != null && element.VisibleInHierarchy)
            focusController.ScrollTo(element.Container.transform, smooth);
    }

    /// <summary>
    /// Causes the content pane to resize itself to fit its children during the next layout update.
    /// </summary>
    /// <remarks>
    /// This is an expensive operation and should only be used when menu elements are
    /// repositioned/resized in a way that the ScrollingGroup does not already detect.
    /// </remarks>
    public void QueueContentResize() => resizeQueued = true;

    /// <inheritdoc/>
    public void UpdateLayout(Vector2 localAnchorPos)
    {
        AddScrollNavHelpers();

        if (resizeQueued)
        {
            resizeQueued = false;

            var contentRT = (RectTransform)contentPane.transform;

            contentRT.sizeDelta = Vector2.zero;
            Content?.UpdateLayout(Vector2.zero);
            var (min, max) = CalculateContentBounds();

            contentRT.sizeDelta = max - min;
            contentAnchor = contentRT.sizeDelta * 0.5f - max;
            anchorOffset = new Vector2(0, max.y);
        }

        ((RectTransform)scrollPane.transform).anchoredPosition = localAnchorPos + anchorOffset;
        Content?.ClearNeighbors();
        Content?.UpdateLayout(contentAnchor);
    }

    /// <inheritdoc/>
    public void SetGameObjectParent(GameObject parent) =>
        scrollPane.transform.SetParentReset(parent.transform);

    /// <inheritdoc/>
    public void ClearGameObjectParent() => scrollPane.transform.SetParent(null);

    #endregion
    #region Internals

    private readonly ScrollFocusController focusController;
    private bool resizeQueued = true;
    private Vector2 contentAnchor,
        anchorOffset;

    /// <summary>
    /// The GameObject acting as a scrolling viewport for the <see cref="contentPane"/>.
    /// </summary>
    protected readonly GameObject scrollPane;

    /// <summary>
    /// The GameObject parent for <see cref="Content"/>.
    /// </summary>
    protected readonly GameObject contentPane;

    private void AddScrollNavHelpers()
    {
        foreach (var element in Content?.AllElements().OfType<SelectableElement>() ?? [])
        {
            // This component removes itself when the object it's attached to is re-parented
            element
                .SelectableComponent.gameObject.AddComponentIfNotPresent<ScrollNavigationHelper>()
                .container = element.Container.transform;
        }
    }

    /// <summary>
    /// Calculates the cumulative bounds of all children of the content pane in local coordinates,
    /// skipping over anything that is contained within another ScrollRect.
    /// </summary>
    private (Vector2 min, Vector2 max) CalculateContentBounds()
    {
        Vector3 min = Vector3.one * float.MaxValue,
            max = Vector3.one * float.MinValue;
        Matrix4x4 worldToLocal = contentPane.transform.worldToLocalMatrix;

        foreach (
            Transform descendant in EnumerateDescendantsConditional(
                contentPane.transform,
                shouldSkip: ChildrenOfScrollPanesExceptScrollbars
            )
        )
        {
            if (descendant is not RectTransform rt || !descendant.gameObject.activeInHierarchy)
                continue;
            foreach (Vector3 corner in rt.GetCorners())
            {
                Vector3 localCorner = worldToLocal.MultiplyPoint3x4(corner);
                min = Vector3.Min(localCorner, min);
                max = Vector3.Max(localCorner, max);
            }
        }

        return (min, max);

        static bool ChildrenOfScrollPanesExceptScrollbars(Transform x) =>
            x.parent.TryGetComponent<ScrollSliderController>(out var sliderCtrl)
            && (!sliderCtrl.VerticalSlider || x != sliderCtrl.VerticalSlider.transform)
            && (!sliderCtrl.HorizontalSlider || x != sliderCtrl.HorizontalSlider.transform);
    }

    /// <summary>
    /// Enumerates the entire Transform hierarchy, skipping branches when it finds
    /// a transform for which the given <paramref name="shouldSkip"/> predicate is true.
    /// </summary>
    private static IEnumerable<Transform> EnumerateDescendantsConditional(
        Transform transform,
        Func<Transform, bool> shouldSkip
    )
    {
        foreach (Transform item in transform)
        {
            if (shouldSkip(item))
                continue;
            yield return item;
            foreach (var item2 in EnumerateDescendantsConditional(item, shouldSkip))
                yield return item2;
        }
    }

    #endregion
    #region Methods that delegate to Content

    /// <inheritdoc/>
    public SelectableElement? GetDefaultSelectable() => Content?.GetDefaultSelectable();

    /// <inheritdoc/>
    public IEnumerable<MenuElement> AllElements() => Content?.AllElements() ?? [];

    /// <inheritdoc/>
    public void ClearNeighbors() => Content?.ClearNeighbors();

    /// <inheritdoc/>
    public void SetNeighbors(NavigationDirection direction, IEnumerable<Selectable> selectables) =>
        Content?.SetNeighbors(direction, selectables);

    /// <inheritdoc/>
    public void ClearNeighbors(NavigationDirection direction) => Content?.ClearNeighbors(direction);

    /// <inheritdoc/>
    public bool GetSelectables(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out IEnumerable<Selectable> selectables
    )
    {
        selectables = default;
        return Content?.GetSelectables(direction, out selectables) ?? false;
    }

    #endregion
}
