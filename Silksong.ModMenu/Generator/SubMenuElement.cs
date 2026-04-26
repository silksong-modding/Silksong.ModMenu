using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenu.Generator;

/// <summary>
/// A text button which opens a sub-menu of type M, for data of type T.
/// </summary>
public class SubMenuElement<T, M> : TextButton
    where M : ICustomMenu<T>
{
    /// <summary>
    /// Sub menu abstraction shown by this button.
    /// </summary>
    public readonly M SubMenu;

    /// <summary>
    /// Construct a new SubMenuElement.
    /// </summary>
    public SubMenuElement(LocalizedText text, M subMenu, LocalizedText? description = null)
        : base(text, description ?? "")
    {
        SubMenu = subMenu;

        PaginatedMenuScreenBuilder builder = new(text);
        builder.AddRange(SubMenu.Elements());
        var screen = builder.Build();

        OnSubmit += () => MenuScreenNavigation.Show(screen);
    }
}
