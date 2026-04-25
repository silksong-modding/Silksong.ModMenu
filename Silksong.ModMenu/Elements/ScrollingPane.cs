using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A vertically scrolling panel that can contain an arbitrary amount of content.
/// </summary>
public class ScrollingPane : MenuDisposable, INavigableMenuEntity
{
    #region API

    /// <inheritdoc/>
    public VisibilityManager Visibility { get; } = new(false);

    /// <summary>
    /// The scrolling pane's content.
    /// This entity cannot be a <see cref="ScrollingPane"/>,
    /// and cannot contain any <see cref="ScrollingPane"/> children.
    /// </summary>
    public INavigableMenuEntity? Content
    {
        get => field;
        set
        {
            if (field == value)
                return;
            if (value?.GetType().IsSubclassOf(typeof(ScrollingPane)) ?? false)
                throw new System.ArgumentException(
                    $"A {nameof(ScrollingPane)} cannot have another {nameof(ScrollingPane)} as its direct content entity.",
                    value.GetType().FullName
                );

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

        OnDispose += () => Object.Destroy(scrollPane);
        Visibility.OnVisibilityChanged += visibleInHierarchy =>
            scrollPane.SetActive(visibleInHierarchy);

        scrollPane.SetActive(true);
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
    public void QueueContentResize() => resizeQueued = true;

    /// <inheritdoc/>
    public void UpdateLayout(Vector2 localAnchorPos)
    {
        AddScrollNavHelpers();

        if (resizeQueued)
        {
            resizeQueued = false;
            foreach (Transform child in contentPane.transform)
            {
                if (child.GetComponent<ScrollRect>())
                    throw new System.InvalidOperationException(
                        $"A {nameof(ScrollingPane)} cannot have other scrolling views nested within it."
                    );
            }

            var contentRT = (RectTransform)contentPane.transform;

            contentRT.sizeDelta = Vector2.zero;
            Content?.UpdateLayout(Vector2.zero);
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(contentRT);
            Vector2 min = bounds.min,
                max = bounds.max;

            contentRT.sizeDelta = max - min;
            contentAnchor = contentRT.sizeDelta / 2f - max;
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
            // This component removes itself when the object it's attached to is re-parented
            element.SelectableComponent.gameObject.AddComponentIfNotPresent<ScrollNavigationHelper>();
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
    public void SetNeighbor(NavigationDirection direction, Selectable selectable) =>
        Content?.SetNeighbor(direction, selectable);

    /// <inheritdoc/>
    public void ClearNeighbor(NavigationDirection direction) => Content?.ClearNeighbor(direction);

    /// <inheritdoc/>
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
