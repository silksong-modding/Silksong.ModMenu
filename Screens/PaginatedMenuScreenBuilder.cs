using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using UnityEngine;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A convience class for building a paginated menu screen from a stream of elements, grouping them into VerticalGroups.
/// </summary>
public class PaginatedMenuScreenBuilder(string title, int pageSize = 8)
{
    private readonly string title = title;
    private readonly int pageSize = pageSize;
    private readonly List<MenuElement> menuElements = [];

    /// <summary>
    /// Top anchor point for all pages.
    /// </summary>
    public Vector2 Anchor = SpacingConstants.TOP_CENTER_ANCHOR;

    /// <summary>
    /// Vertical spacing on each page.
    /// </summary>
    public float VerticalSpacing = SpacingConstants.VSPACE_MEDIUM;

    public void Add(MenuElement menuElement) => menuElements.Add(menuElement);

    public void AddRange(IEnumerable<MenuElement> menuElements) =>
        this.menuElements.AddRange(menuElements);

    public PaginatedMenuScreen Build()
    {
        PaginatedMenuScreen screen = new(title) { Anchor = Anchor };
        foreach (var list in menuElements.Partition(pageSize))
        {
            VerticalGroup group = new() { VerticalSpacing = VerticalSpacing };
            group.AddRange(list);
            screen.AddPage(group);
        }

        return screen;
    }
}
