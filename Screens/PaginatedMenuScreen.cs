using System;
using System.Collections.Generic;
using System.Linq;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A menu screen which paginates across elements through a choice panel at the bottom.
/// </summary>
public class PaginatedMenuScreen : AbstractMenuScreen
{
    private readonly List<Page> pages = [];

    private readonly IntRangeChoiceModel pageNumberModel;
    private readonly ChoiceElement<int> pageNumberElement;

    public PaginatedMenuScreen(string title, int pageSize = 8)
        : base(title)
    {
        if (pageSize < 1)
            throw new ArgumentException($"{nameof(pageSize)} ({pageSize}) must be >= 1");
        PageSize = pageSize;

        pageNumberModel = new(0, 0, 0) { Circular = true, DisplayFn = i => $"{i + 1}" };
        pageNumberModel.OnValueChanged += _ => UpdateLayout();

        pageNumberElement = new("Page", pageNumberModel);
        pageNumberElement.AddToContainer(ControlsPane);

        var pos = BackButton.GetComponent<RectTransform>().anchoredPosition;
        pageNumberElement.RectTransform.SetAnchoredPosition(pos);
        pos.y -= SpacingConstants.VSPACE_MEDIUM;
        BackButton.GetComponent<RectTransform>().SetAnchoredPosition(pos);

        UpdateLayout();
    }

    /// <summary>
    /// The maximum number of elements on each page.
    /// </summary>
    public readonly int PageSize;

    /// <summary>
    /// The currently selected page number, 1-based.
    /// </summary>
    public int PageNumber
    {
        get => pageNumberModel.Value;
        set => pageNumberModel.Value = value;
    }

    /// <summary>
    /// The number of pages on this menu screen.
    /// </summary>
    public int PageCount => pages.Count;

    /// <summary>
    /// Top anchor point for the elements.
    /// </summary>
    public Vector2 Anchor
    {
        get => field;
        set => MaybeUpdateLayout(ref field, value);
    } = SpacingConstants.TOP_CENTER_ANCHOR;

    /// <summary>
    /// Vertical space between rendered elements.
    /// </summary>
    public float VerticalSpacing
    {
        get => field;
        set => MaybeUpdateLayout(ref field, value);
    } = SpacingConstants.VSPACE_MEDIUM;

    /// <summary>
    /// Add a singular element to the list of pages.
    /// </summary>
    public void Add(MenuElement menuElement) => Add([menuElement]);

    /// <summary>
    /// Add a range of elements to the list of pages.
    /// </summary>
    public void Add(IEnumerable<MenuElement> menuElements)
    {
        foreach (var element in menuElements)
            AddToPages(element);

        UpdateLayout();
    }

    private Page? ActivePage => pages.Count > 0 ? pages[PageNumber] : null;

    private IEnumerable<SelectableElement> GetActiveSelectables()
    {
        foreach (var e in ActivePage?.Elements ?? [])
        {
            if (e is not SelectableElement s)
                continue;
            if (!s.Visible || !s.Interactable)
                continue;

            yield return s;
        }

        if (PageCount > 1)
            yield return pageNumberElement;
    }

    protected override SelectableElement? GetDefaultSelectableInternal() =>
        GetActiveSelectables().FirstOrDefault();

    protected override void UpdateLayout()
    {
        if (pages.Count == 0)
            return; // Nothing to update.

        // Update the page number model.
        pageNumberModel.ResetParams(0, pages.Count - 1, pageNumberElement.Value);
        pageNumberElement.Visible = pages.Count > 1;

        // Set the active page.
        for (int i = 0; i < pages.Count; i++)
            pages[i].Container.SetActive(i == PageNumber);

        // Set spacing for the active page.
        Vector2 pos = Anchor;
        foreach (var element in ActivePage?.Elements ?? [])
        {
            element.RectTransform.SetAnchoredPosition(pos);
            pos.y -= VerticalSpacing;
        }

        // Set navigation for the active page.
        List<Selectable> column = [.. GetActiveSelectables().Select(s => s.SelectableComponent)];
        column.Add(BackButton);

        foreach (var s in column)
            s.ClearNav();
        foreach (var (top, bot) in column.CircularPairs())
        {
            top.SetNavDown(bot);
            bot.SetNavUp(top);
        }
    }

    private record Page(GameObject Container)
    {
        internal readonly List<MenuElement> Elements = [];
    }

    private void AddToPages(MenuElement element)
    {
        if (element is SelectableElement s)
            RegisterSelectable(s);

        Page page;
        if (pages.Count == 0 || pages.Last().Elements.Count == PageSize)
        {
            GameObject container = MenuPrefabs.Get().NewEmptyContentPane();
            container.name = $"Page {pages.Count + 1}";
            container.transform.SetParent(ContentPane.transform);
            container.transform.localScale = Vector3.one;
            container.transform.localPosition = Vector3.zero;
            container.GetComponent<RectTransform>().FitToParent();

            page = new(container);
            pages.Add(page);
        }
        else
            page = pages.Last();

        page.Elements.Add(element);
        element.AddToContainer(page.Container);
    }
}
