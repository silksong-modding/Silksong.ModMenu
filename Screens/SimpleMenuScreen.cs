using System.Collections.Generic;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A basic menu screen with a single VerticalGroup for the content panel. Sufficient for simple menus with 8 or fewer elements of standard size.
/// </summary>
public class SimpleMenuScreen(string title) : BasicMenuScreen(title, new VerticalGroup())
{
    /// <summary>
    /// The content pane for this menu screen.
    /// </summary>
    public new VerticalGroup Content => (VerticalGroup)base.Content;

    /// <summary>
    /// Add a menu element to this screen.
    /// </summary>
    public void Add(MenuElement menuElement) => Content.Add(menuElement);

    /// <summary>
    /// Add a range of menu elements to this screen.
    /// </summary>
    public void AddRange(IEnumerable<MenuElement> menuElements) => Content.AddRange(menuElements);
}
