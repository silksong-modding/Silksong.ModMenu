using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A layout group which wraps a scrollable viewport around any layout.
/// Scrolling layout groups can only contain atomic <see cref="MenuElement"/>s.
/// </summary>
/// <typeparam name="TGroup">
/// A descendant of <see cref="AbstractGroup"/> which this group uses internally to control the
/// layout of its elements. This type CANNOT be a descendant of <see cref="ScrollingGroup{TGroup}"/>.
/// </typeparam>
public class ScrollingGroup : MenuDisposable, INavigableMenuEntity
{
    private readonly VisibilityManager visibility = new(false);

    #region API

    public INavigableMenuEntity? Content
    {
        get => field;
        set
        {
            if (field == value)
                return;

            field?.ClearParents();
            field = value;
            field?.SetParents(this, scrollPane);
        }
    }

    /// <summary>
    /// The <see cref="ScrollRect"/> component that controls the
    /// scrolling sensitivity and movement type.
    /// </summary>
    public readonly ScrollRect scrollRect;

    /// <summary>
    /// Construct a new scrolling group that displays the contents of <paramref name="layout"/>.
    /// </summary>
    /// <param name="layout"><inheritdoc cref="Layout" path="//summary"/></param>
    public ScrollingGroup(INavigableMenuEntity content)
        : base()
    {
        // Generic constraints don't have a not operator, so we enforce this at runtime.
        if (content?.GetType().IsSubclassOf(typeof(ScrollingGroup)) ?? false)
            throw new ArgumentException(
                "A scrolling group cannot use another scrolling group for its layout.",
                content.GetType().FullName
            );

        scrollPane = MenuPrefabs
            .Get()
            .NewScrollPane(out scrollRect, out focusController, out contentPane);
        ((RectTransform)scrollPane.transform).pivot = new Vector2(0.5f, 1);

        contentPane.AddComponent<OnChildTransformsChangeHelper>().OnChildrenChanged +=
            QueueContentResize;

        Content = content;
        ValidateElementsAndAddHelpers();
        visibility.OnVisibilityChanged += visibleInHierarchy =>
            scrollPane.SetActive(visibleInHierarchy);
    }

    /// <summary>
    /// The size of the visible area of the scroll pane.
    /// </summary>
    public Vector2 ViewportSize
    {
        get => scrollRect.viewport.sizeDelta;
        set => scrollRect.viewport.sizeDelta = value;
    }

    /// <summary>
    /// The time, in seconds, that it takes to smooth-scroll between menu elements.
    /// </summary>
    public float SmoothScrollTime
    {
        get => focusController.smoothScrollTime;
        set => focusController.smoothScrollTime = value;
    }

    public VisibilityManager Visibility => visibility;

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
            focusController.ScrollTo(element.RectTransform, smooth);
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
            focusController.ScrollTo(element.RectTransform, smooth);
    }

    /// <summary>
    /// Causes the content pane to resize itself to fit its children during the next layout update.
    /// </summary>
    /// <remarks>
    /// This is an expensive operation and should only be used when menu elements are
    /// repositioned/resized in a way that the ScrollingGroup does not already detect.
    /// </remarks>
    public void QueueContentResize() => resizeQueued = !resizing;

    /// <inheritdoc/>
    public void UpdateLayout(Vector2 localAnchorPos)
    {
        ValidateElementsAndAddHelpers();

        if (resizeQueued)
        {
            ResizeContentPane();
            resizeQueued = false;
        }

        var scrollRT = (RectTransform)scrollPane.transform;
        var size = scrollRT.sizeDelta;

        scrollRT.anchoredPosition = localAnchorPos + anchorOffset;
        scrollRT.sizeDelta = size;

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
    private bool resizeQueued = false,
        resizing = false;
    private Vector2 contentAnchor,
        anchorOffset;

    /// <summary>
    /// The GameObject acting as a scrolling viewport for the <see cref="contentPane"/>.
    /// </summary>
    protected readonly GameObject scrollPane;

    /// <summary>
    /// The GameObject containing all menu elements in this group.
    /// </summary>
    protected readonly GameObject contentPane;

    /// <summary>
    /// Resizes the content pane to fit the height and width of all elements inside it.
    /// </summary>
    private void ResizeContentPane()
    {
        resizing = true;
        var contentRT = (RectTransform)contentPane.transform;

        contentRT.sizeDelta = Vector2.zero;
        Content?.UpdateLayout(Vector2.zero);
        var (min, max) = contentRT.GetRelativeBoundsOf(
            Content?.AllElements().OfType<MenuElement>().Select(x => x.Container.transform) ?? []
        );

        contentRT.sizeDelta = max - min;
        contentAnchor = contentRT.sizeDelta / 2f - max;
        anchorOffset = new Vector2(0, max.y);
        resizing = false;
    }

    private void ValidateElementsAndAddHelpers()
    {
        foreach (var element in Content?.AllElements().OfType<SelectableElement>() ?? [])
        {
            element.SelectableComponent.gameObject.AddComponentIfNotPresent<ScrollNavigationHelper>();
        }
    }

    #endregion
    #region Overrides that just delegate to Layout

    public SelectableElement? GetDefaultSelectable() => Content?.GetDefaultSelectable();

    public IEnumerable<MenuElement> AllElements() => Content?.AllElements() ?? [];

    public void ClearNeighbors() => Content?.ClearNeighbors();

    public void SetNeighbor(NavigationDirection direction, Selectable selectable) =>
        Content?.SetNeighbor(direction, selectable);

    public void ClearNeighbor(NavigationDirection direction) => Content?.ClearNeighbor(direction);

    public bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    )
    {
        selectable = default;
        return Content?.GetSelectable(direction, out selectable) ?? false;
    }
    #endregion
}
