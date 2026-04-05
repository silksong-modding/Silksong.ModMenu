using BepInEx;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Plugin;

namespace Silksong.ModMenuTesting;

// This test requires its own ConfigFile so it's not part of the general testing framework.
[BepInAutoPlugin(id: "org.silksong_modding.modmenunestedtesting")]
public partial class ModMenuNestedTestingPlugin : BaseUnityPlugin, IModMenuNestedMenu
{
    private void Awake()
    {
        Config.Bind(new("Main", "Int1"), 1, new("Root Integer 1"));
        Config.Bind(new("Main", "Int2"), 2, new("Root Integer 2"));
        // Place this in Main instead of its config path.
        Config.Bind(
            new("Other.Group", "Int3"),
            3,
            new("Other Integer 3", tags: new ConfigEntrySubgroup(["Main", "Int3"]))
        );

        // We should generte a single 'Sub' button for the three below
        Config.Bind(new("Main.Sub", "Int4"), 4, new("Sub Integer 4"));
        Config.Bind(new("Main.Sub", "Int5"), 5, new("Sub Integer 5"));
        // This gets flattened into the 'Sub' menu because there are not enough 'Sub.Sub' elements.
        Config.Bind(new("Main.Sub.Sub", "Int6"), 6, new("Sub Sub Integer 6"));
    }

    public LocalizedText ModMenuName() => "Mod Menu Nested Testing";
}
