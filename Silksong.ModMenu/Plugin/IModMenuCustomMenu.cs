using Silksong.ModMenu.Screens;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Marker interface for a plugin that provides its own custom sub-menu.
/// </summary>
public interface IModMenuCustomMenu : IModMenuInterface
{
    /// <summary>
    /// Generate a sub-menu unique to this mod. The Mods menu will open this sub-menu when the mod is selected.
    /// </summary>
    public AbstractMenuScreen BuildCustomMenu();
}
