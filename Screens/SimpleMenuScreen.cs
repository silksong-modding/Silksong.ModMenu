using System.Collections.Generic;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A basic menu screen with a single VerticalGroup for the content panel. Sufficient for most simple menus with not many elements.
/// </summary>
public class SimpleMenuScreen(string title) : BasicMenuScreen(title, new VerticalGroup())
{
    public new VerticalGroup Content => (VerticalGroup)base.Content;

    public void Add(MenuElement menuElement) => Content.Add(menuElement);

    public void AddRange(IEnumerable<MenuElement> menuElements) => Content.AddRange(menuElements);
}
