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
public class ScrollingGroup<TGroup> : AbstractGroup
    where TGroup : AbstractGroup
{
    #region API

    /// <summary>
    /// The scrolling group's layout object.
    /// Use to add to, remove from, or update this group's <see cref="MenuElement"/>s.
    /// </summary>
    public readonly TGroup Layout;

    /// <summary>
    /// The <see cref="ScrollRect"/> component that controls the
    /// scrolling sensitivity and movement type.
    /// </summary>
    public readonly ScrollRect scrollRect;

    /// <summary>
    /// Construct a new scrolling group that displays the contents of <paramref name="layout"/>.
    /// </summary>
    /// <param name="layout"><inheritdoc cref="Layout" path="//summary"/></param>
    public ScrollingGroup(TGroup layout)
        : base()
    {
        // Generic constraints don't have a not operator, so we enforce this at runtime.
        if (typeof(TGroup).IsSubclassOfRawGeneric(typeof(ScrollingGroup<>)))
            throw new ArgumentException(
                "A scrolling group cannot use another scrolling group for its layout.",
                nameof(TGroup)
            );

        scrollPane = MenuPrefabs
            .Get()
            .NewScrollPane(out scrollRect, out focusController, out contentPane);
        ((RectTransform)scrollPane.transform).pivot = new Vector2(0.5f, 1);

        contentPane.AddComponent<OnChildTransformsChangeHelper>().OnChildrenChanged +=
            QueueContentResize;

        Layout = layout;
        Layout.SetParents(this, contentPane);
        OnDispose += () => Layout.Dispose();

        ValidateElementsAndAddHelpers();

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
        if (Contains(element) && element.VisibleInHierarchy)
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
        var element = (MenuElement?)AllEntities().ElementAtOrDefault(index);
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
    public override void UpdateLayout(Vector2 localAnchorPos)
    {
        if (Visibility.VisibleInHierarchy && !Layout.Visibility.VisibleInHierarchy)
            Layout.SetParents(this, contentPane);

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

        ClearNeighbors();
        Layout.UpdateLayout(contentAnchor);
    }

    /// <inheritdoc/>
    public override void SetGameObjectParent(GameObject parent)
    {
        ClearGameObjectParent();
        gameObjectParent = parent;
        scrollPane.transform.SetParentReset(gameObjectParent.transform);
    }

    /// <inheritdoc/>
    public override void ClearGameObjectParent()
    {
        gameObjectParent = null;
        scrollPane.transform.SetParent(null);
    }

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
        Layout.UpdateLayout(Vector2.zero);
        var (min, max) = contentRT.GetRelativeBoundsOf(
            AllEntities().OfType<MenuElement>().Select(x => x.Container.transform)
        );

        contentRT.sizeDelta = max - min;
        contentAnchor = contentRT.sizeDelta / 2f - max;
        anchorOffset = new Vector2(0, max.y);
        resizing = false;
    }

    private void ValidateElementsAndAddHelpers()
    {
        foreach (var element in AllEntities())
        {
            if (element is not MenuElement)
            {
                throw new InvalidOperationException(
                    $"Scrolling groups may only contain atomic menu elements; {element.GetType()} cannot be nested inside this group."
                );
            }
            if (element is SelectableElement selectable)
                selectable.SelectableComponent.gameObject.AddComponentIfNotPresent<ScrollNavigationHelper>();
        }
    }

    #endregion
    #region Overrides that just delegate to Layout

    /// <inheritdoc/>
    public override IEnumerable<IMenuEntity> AllEntities() => Layout.AllEntities();

    /// <inheritdoc/>
    public override void Clear() => Layout.Clear();

    /// <inheritdoc/>
    public override bool Contains(IMenuEntity entity) => Layout.Contains(entity);

    /// <inheritdoc/>
    protected internal override void AddChild(IMenuEntity element) => Layout.AddChild(element);

    /// <inheritdoc/>
    public override bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    ) => Layout.GetSelectable(direction, out selectable);

    /// <inheritdoc/>
    protected internal override IEnumerable<INavigable> GetNavigables(
        NavigationDirection direction
    ) => Layout.GetNavigables(direction);

    #endregion
}
