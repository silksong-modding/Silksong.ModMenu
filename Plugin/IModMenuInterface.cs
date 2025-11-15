namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Marker interface for all plugin interfaces intended to generate mod menus.
/// </summary>
public interface IModMenuInterface
{
    /// <summary>
    /// A unique identifier for this mod menu, used as a case-insensitive sort key.
    /// </summary>
    public string ModMenuName();
}
