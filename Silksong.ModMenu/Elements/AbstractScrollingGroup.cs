using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Base class for scrolling layout groups. Scrolling layout groups can only contain atomic menu elements.
/// </summary>
/// <typeparam name="TGroup">
/// A descendant of <see cref="AbstractGroup"/> which this group uses internally to control the
/// layout of its elements. This type CANNOT be a descendant of <see cref="AbstractScrollingGroup{TGroup}"/>.
/// </typeparam>
public abstract class AbstractScrollingGroup<TGroup> : AbstractGroup
    where TGroup : AbstractGroup
{
    #region Internals

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
    /// Used internally to control the layout of the group's menu elements.
    /// Direct references to this object MUST stay internal to this scrolling group.
    /// </summary>
    protected TGroup InternalGroup { get; private init; }

    /// <summary>
    /// Construct an empty scrolling group.
    /// </summary>
    protected AbstractScrollingGroup(TGroup internalGroup)
        : base()
    {
        // Generic constraints don't have a not operator, so we enforce this at runtime.
        if (typeof(TGroup).IsSubclassOfRawGeneric(typeof(AbstractScrollingGroup<>)))
            throw new ArgumentException(
                "A scrolling group cannot use another scrolling group for its layout.",
                nameof(TGroup)
            );

        scrollPane = MenuPrefabs.Get().NewScrollPane(out scrollRect, out contentPane);
        scrollPane.SetActive(true);
        ((RectTransform)scrollPane.transform).pivot = new Vector2(0.5f, 1);

        InternalGroup = internalGroup;
        InternalGroup.SetParents(this, contentPane);
        OnDispose += () => InternalGroup.Dispose();

        Visibility.OnVisibilityChanged += visibleInHierarchy =>
            scrollPane.SetActive(visibleInHierarchy);
    }

    /// <summary>
    /// Resizes the content pane to fit the height and width of all elements inside it.
    /// </summary>
    /// <remarks>
    /// This is an expensive operation and should NOT be called in UpdateLayout; but
    /// it SHOULD be called whenever elements are removed from the group, or
    /// any other changes occur which would afect the cumulative size of the elements.
    /// By default, this is already called in <see cref="AddChild"/>.
    /// </remarks>
    protected void RecalculateContentPaneSize()
    {
        var contentRT = (RectTransform)contentPane.transform;

        contentRT.sizeDelta = Vector2.zero;
        InternalGroup.UpdateLayout(Vector2.zero);
        var (min, max) = contentRT.GetRelativeBoundsOf(
            AllEntities().OfType<MenuElement>().Select(x => x.Container.transform)
        );

        contentRT.sizeDelta = max - min;
        contentAnchor = contentRT.sizeDelta / 2f - max;
        anchorOffset = new Vector2(0, max.y);
    }

    /// <inheritdoc/>
    protected internal override void AddChild(IMenuEntity element)
    {
        if (element is not MenuElement)
            throw new ArgumentException(
                "Scrolling groups can only contain atomic menu elements.",
                nameof(element)
            );
        InternalGroup.AddChild(element);
        if (element is SelectableElement selectableElt)
            selectableElt
                .SelectableComponent.gameObject.AddComponentIfNotPresent<ScrollNavigationHelper>()
                .ScrollRect = scrollRect;
        RecalculateContentPaneSize();
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<INavigable> GetNavigables(
        NavigationDirection direction
    ) => InternalGroup.GetNavigables(direction);

    #endregion
    #region API

    /// <summary>
    /// The <see cref="ScrollRect"/> component that controls the
    /// scrolling sensitivity and movement type.
    /// </summary>
    public readonly ScrollRect scrollRect;

    /// <summary>
    /// The size of the visible area of the scroll pane.
    /// </summary>
    public Vector2 ViewportSize
    {
        get => scrollRect.viewport.sizeDelta;
        set => scrollRect.viewport.sizeDelta = value;
    }

    /// <summary>
    /// If the given SelectableElement belongs to this group, scroll it into view.
    /// </summary>
    /// <param name="selectable">The element to scroll to.</param>
    /// <param name="smooth">
    ///     If false, the scroll pane position will change instantly.
    ///     If true, the element will move into view smoothly over a short time.
    /// </param>
    public void ScrollTo(SelectableElement selectable, bool smooth = false)
    {
        if (Contains(selectable) && selectable.VisibleInHierarchy)
        {
            var navHelper = selectable.SelectableComponent.GetComponent<ScrollNavigationHelper>();
            if (smooth)
                navHelper.ScrollToSmooth();
            else
                navHelper.ScrollToInstant();
        }
    }

    /// <summary>
    /// If the menu element at the given <paramref name="index"/> is a
    /// <see cref="SelectableElement"/>, scrolls it into view.
    /// </summary>
    /// <param name="index">The index of the element to scroll to.</param>
    /// <param name="smooth">
    ///     If false, the scroll pane position will change instantly.
    ///     If true, the element will move into view smoothly over a short time.
    /// </param>
    public void ScrollTo(int index, bool smooth = false)
    {
        var elem = AllEntities().ElementAtOrDefault(index);
        if (elem is SelectableElement selectable)
            ScrollTo(selectable, smooth);
    }

    /// <inheritdoc/>
    public override void UpdateLayout(Vector2 localAnchorPos)
    {
        ClearNeighbors();

        foreach (var selectableElt in AllEntities().OfType<SelectableElement>())
        {
            selectableElt
                .SelectableComponent.gameObject.AddComponentIfNotPresent<ScrollNavigationHelper>()
                .ScrollRect = scrollRect;
        }

        var scrollRT = (RectTransform)scrollPane.transform;
        var size = scrollRT.sizeDelta;

        scrollRT.anchoredPosition = localAnchorPos + anchorOffset;
        scrollRT.sizeDelta = size;

        InternalGroup.UpdateLayout(contentAnchor);
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

    /// <inheritdoc/>
    public override IEnumerable<IMenuEntity> AllEntities() => InternalGroup.AllEntities();

    /// <inheritdoc/>
    public override void Clear() => InternalGroup.Clear();

    /// <inheritdoc/>
    public override bool Contains(IMenuEntity entity) => InternalGroup.Contains(entity);

    /// <inheritdoc/>
    public override bool GetSelectable(
        NavigationDirection direction,
        [MaybeNullWhen(false)] out Selectable selectable
    ) => InternalGroup.GetSelectable(direction, out selectable);

    #endregion
}
