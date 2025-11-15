namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Marker interface for a plugin that has a simple 'Enabled/Disabled' setting.
/// </summary>
public interface IModMenuToggle : IModMenuInterface
{
    /// <summary>
    /// Returns whether this mod is currently enabled.
    /// </summary>
    public bool ModMenuGetEnabled();

    /// <summary>
    /// Callback to change the 'enabled' state of the mod when modified in the menu.
    /// </summary>
    public void ModMenuSetEnabled(bool enabled);

    /// <summary>
    /// Description text shown under the mod name.
    /// </summary>
    public virtual string ModMenuDescription() => "";
}
