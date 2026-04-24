using System.Collections.Generic;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A menu screen that provides a vertically scrolling view of a single column of elements.
/// Sufficient for simple menus with an arbitrary number of elements.
/// </summary>
public class ScrollingMenuScreen(LocalizedText title)
    : BasicMenuScreen(title, new ScrollingGroup<VerticalGroup>(new()))
{
    /// <summary>
    /// The scrolling viewport for this menu screen.
    /// </summary>
    public ScrollingGroup<VerticalGroup> ScrollView => (ScrollingGroup<VerticalGroup>)base.Content;

    /// <inheritdoc cref="BasicMenuScreen.Content"/>
    public new VerticalGroup Content => ScrollView.Layout;

    /// <summary>
    /// Add a menu element to this screen.
    /// </summary>
    public void Add(MenuElement menuElement) => Content.Add(menuElement);

    /// <summary>
    /// Add multiple menu elements to this screen.
    /// </summary>
    public void AddRange(IEnumerable<MenuElement> menuElements) => Content.AddRange(menuElements);
}
