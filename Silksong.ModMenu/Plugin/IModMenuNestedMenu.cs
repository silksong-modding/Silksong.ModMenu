namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Marker interface for a mod menu that generates hierarchical submenus, based on config keys.
/// </summary>
public interface IModMenuNestedMenu : IModMenuInterface
{
    /// <summary>
    /// The minimum number of elements required in a subgroup to generate a screen for it.
    /// If a sub-page has too few elements, it is flattened into its containing page.
    /// </summary>
    int MinSubgroupSize() => 2;
}
