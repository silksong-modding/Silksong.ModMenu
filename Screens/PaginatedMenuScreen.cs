using System.Collections.Generic;
using System.Linq;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using UnityEngine;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A menu screen which paginates across elements through a choice panel at the bottom.
/// </summary>
public class PaginatedMenuScreen : AbstractMenuScreen
{
    private readonly List<INavigableMenuEntity> pages = [];

    private readonly IntRangeChoiceModel pageNumberModel;
    private readonly ChoiceElement<int> pageNumberElement;

    public PaginatedMenuScreen(string title)
        : base(title)
    {
        pageNumberModel = new(0, 0, 0) { Circular = true, DisplayFn = i => $"{i + 1}" };
        pageNumberModel.OnValueChanged += _ => UpdateLayout();

        pageNumberElement = new("Page", pageNumberModel);
        pageNumberElement.SetGameObjectParent(ControlsPane);

        var pos = BackButton.GetComponent<RectTransform>().anchoredPosition;
        pageNumberElement.RectTransform.SetAnchoredPosition(pos);
        pos.y -= SpacingConstants.VSPACE_MEDIUM;
        BackButton.GetComponent<RectTransform>().SetAnchoredPosition(pos);

        UpdateLayout();
    }

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
    /// Top anchor point for all pages.
    /// </summary>
    public Vector2 Anchor = SpacingConstants.TOP_CENTER_ANCHOR;

    protected override IEnumerable<MenuElement> AllElements() =>
        pages.SelectMany(p => p.AllElements()).Concat([pageNumberElement]);

    /// <summary>
    /// Add a singular page to the list of pages.
    /// </summary>
    public void AddPage(INavigableMenuEntity page)
    {
        pages.Add(page);
        page.SetGameObjectParent(ContentPane);
    }

    /// <summary>
    /// Add multiple pages to the list of pages.
    /// </summary>
    public void AddPages(IEnumerable<INavigableMenuEntity> pages)
    {
        foreach (var page in pages)
            AddPage(page);
    }

    private INavigableMenuEntity? ActivePage => pages.Count > 0 ? pages[PageNumber] : null;

    protected override SelectableElement? GetDefaultSelectableInternal() =>
        ActivePage?.GetDefaultSelectable();

    protected override void UpdateLayout()
    {
        if (pages.Count == 0)
            return; // Nothing to update.

        // Update the page number model.
        pageNumberModel.ResetParams(0, pages.Count - 1, pageNumberElement.Value);
        pageNumberElement.VisibleSelf = pages.Count > 1;

        // Set the active page.
        for (int i = 0; i < pages.Count; i++)
            pages[i].VisibleSelf = i == PageNumber;

        // Set spacing for the active page.
        ActivePage?.UpdateLayout(Anchor);

        // Set navigation for the active page.
        List<INavigable> column = [ActivePage!];
        if (pages.Count > 1)
            column.Add(pageNumberElement);
        column.Add(new SelectableWrapper(BackButton));

        foreach (var s in column)
            s.ClearNeighbors();
        foreach (var (top, bot) in column.CircularPairs())
        {
            if (top.GetNeighborUp(out var s))
                bot.SetNeighborUp(s);
            if (bot.GetNeighborDown(out s))
                top.SetNeighborDown(s);
        }
    }
}
