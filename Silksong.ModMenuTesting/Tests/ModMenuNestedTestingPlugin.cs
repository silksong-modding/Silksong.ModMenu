using BepInEx.Configuration;
using Silksong.ModMenu.Plugin;

namespace Silksong.ModMenuTesting;

internal class ModMenuNestedTestingPlugin()
    : BaseProxyPluginTest("org.silksong_modding.modmenunestedtesting")
{
    protected override void Setup(ConfigFile config)
    {
        config.Bind(new("Main", "Int1"), 1, new("Root Integer 1"));
        config.Bind(new("Main", "Int2"), 2, new("Root Integer 2"));
        // Place this in Main instead of its config path.
        config.Bind(
            new("Other.Group", "Int3"),
            3,
            new("Other Integer 3", tags: new ConfigEntrySubgroup(["Main", "Int3"]))
        );

        // We should generte a single 'Sub' button for the three below
        config.Bind(new("Main.Sub", "Int4"), 4, new("Sub Integer 4"));
        config.Bind(new("Main.Sub", "Int5"), 5, new("Sub Integer 5"));
        // This gets flattened into the 'Sub' menu because there are not enough 'Sub.Sub' elements.
        config.Bind(new("Main.Sub.Sub", "Int6"), 6, new("Sub Sub Integer 6"));

        // Test
        config.Bind(new("Main.B.C", "Int7"), 7, new("ABC Integer 7"));
        config.Bind(new("Main.B.C", "Int8"), 8, new("ABC Integer 8"));
    }

    internal override string Name => "Mod Menu Nested Testing";

    protected override ConfigEntryFactory GetFactory()
    {
        return new() { GenerateSubgroups = true };
    }
}
