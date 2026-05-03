using System;
using System.IO;
using BepInEx.Configuration;
using Silksong.ModMenu.Plugin;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting;

internal abstract class BaseProxyPluginTest : ModMenuTest
{
    private ConfigFile config;

    public BaseProxyPluginTest(string pluginId)
    {
        string configFolder = Path.GetDirectoryName(GetType().Assembly.Location);
        config = new(Path.Combine(configFolder, pluginId + ".cfg"), true);

        Setup(config);
    }

    protected virtual ConfigEntryFactory GetFactory() => new();

    protected abstract void Setup(ConfigFile config);

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        ConfigEntryFactory factory = GetFactory();
        if (!factory.GenerateEntryButton(Name, config, out AbstractMenuScreen? screen, out _))
        {
            throw new ArgumentException($"No menu created for {Name}");
        }

        return screen;
    }
}
