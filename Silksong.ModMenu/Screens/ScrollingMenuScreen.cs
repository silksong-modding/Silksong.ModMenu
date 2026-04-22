using System.Collections.Generic;
using System.Linq;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A scrolling menu screen with a single VerticalGroup for the content panel.
/// Sufficient for simple menus with an arbitrary number of elements.
/// </summary>
public class ScrollingMenuScreen : AbstractMenuScreen
{
    #region API

    /// <summary>
    /// Construct a scrolling menu screen with the given title and content.
    /// </summary>
    public ScrollingMenuScreen(LocalizedText title, VerticalGroup content)
        : base(title)
    {
        ScrollPane = MenuPrefabs.Get().NewScrollPane();
        ScrollPane.transform.SetParentReset(Container.transform);
        ScrollPane.SetActive(true);
        ((RectTransform)ScrollPane.transform).anchoredPosition = new Vector2(
            0,
            -0.5f * SpacingConstants.VSPACE_MEDIUM
        );

        ScrollRect = ScrollPane.GetComponent<ScrollRect>();

        Object.Destroy(base.ContentPane);
        ContentPane = ScrollPane.FindChild("Content")!;
        ContentPaneTransform = (RectTransform)ContentPane.transform;

        OnShow += navType =>
        {
            UpdateLayout();
            SelectOnShow(navType).GetComponent<ScrollNavigationHelper>().ScrollToInstant();
        };

        Content = content;
    }

    /// <inheritdoc cref="AbstractMenuScreen.ContentPane"/>
    public new readonly GameObject ContentPane;

    /// <summary>
    /// The <see cref="UnityEngine.UI.ScrollRect"/> component that controls the
    /// scrolling sensitivity and movement type.
    /// </summary>
    public readonly ScrollRect ScrollRect;

    /// <summary>
    /// The content displayed by this menu screen, minus the back button.
    /// </summary>
    public VerticalGroup Content
    {
        get => field;
        set
        {
            if (value == null)
                throw new System.ArgumentNullException(nameof(Content));

            if (field != value && field != null)
            {
                field.ClearParents();
                foreach (
                    var e in field
                        .AllElements()
                        .OfType<SelectableElement>()
                        .Select(x => x.SelectableComponent)
                )
                    if (e.TryGetComponent<ScrollNavigationHelper>(out var navHelper))
                        Object.Destroy(navHelper);
            }

            field = value;
            AddChild(field);
            field.SetGameObjectParent(ContentPane);
        }
    }

    /// <summary>
    /// <inheritdoc cref="ScrollTo" path="//summary"/> smoothly over a short time.
    /// </summary>
    public void ScrollToSmooth(int index) => ScrollTo(index, smooth: true);

    /// <summary>
    /// <inheritdoc cref="ScrollTo" path="//summary"/> instantly.
    /// </summary>
    public void ScrollToInstant(int index) => ScrollTo(index, smooth: false);

    #endregion
    #region Internals

    readonly GameObject ScrollPane;
    readonly RectTransform ContentPaneTransform;
    float size = 0,
        anchor = 0;

    /// <inheritdoc/>
    protected override IEnumerable<IMenuEntity> AllEntities() => [Content];

    /// <inheritdoc/>
    protected override SelectableElement? GetDefaultSelectableInternal() =>
        Content.GetDefaultSelectable();

    /// <inheritdoc/>
    protected override void UpdateLayout()
    {
        MenuElement[] elements =
        [
            .. Content
                .AllElements()
                .Where(x => !Content.HideInactiveElements || x.Visibility.VisibleInHierarchy),
        ];

        // Recalculate the size of ContentPane based on
        // the cumulative height of all visible menu elements.
        ContentPaneTransform.sizeDelta = Vector2.zero;
        Content.UpdateLayout(Vector2.zero);
        var (min, max) = ContentPaneTransform.GetRelativeBoundsOf([
            elements[0].RectTransform,
            elements[^1].RectTransform,
        ]);
        size = max.y - min.y;
        anchor = size / 2f - max.y;

        ContentPaneTransform.sizeDelta = new Vector2(0, size);
        Content.UpdateLayout(new Vector2(0, anchor));

        foreach (var element in elements.OfType<SelectableElement>())
        {
            element
                .SelectableComponent.gameObject.AddComponentIfNotPresent<ScrollNavigationHelper>()
                .ScrollRect = ScrollRect;
        }

        SelectableWrapper wrapper = new(BackButton);
        wrapper.ClearNeighbors();

        Content.SetNeighborDown(BackButton);
        Content.SetNeighborUp(BackButton);
        if (Content.GetNeighborDown(out var selectable))
            wrapper.SetNeighborDown(selectable);
        if (Content.GetNeighborUp(out selectable))
            wrapper.SetNeighborUp(selectable);
    }

    /// <summary>
    /// If the menu element at the given <paramref name="index"/> in
    /// <see cref="Content"/> is a <see cref="SelectableElement"/>,
    /// scrolls it into view
    /// </summary>
    void ScrollTo(int index, bool smooth)
    {
        var elem = Content.AllElements().ElementAtOrDefault(index);
        if (elem is SelectableElement selectable && selectable.VisibleInHierarchy)
        {
            var navHelper = selectable.SelectableComponent.GetComponent<ScrollNavigationHelper>();
            if (smooth)
                navHelper.ScrollToSmooth();
            else
                navHelper.ScrollToInstant();
        }
    }

    #endregion
}
