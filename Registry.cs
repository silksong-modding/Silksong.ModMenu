using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Plugin;

namespace Silksong.ModMenu;

/// <summary>
/// API for extending the standard Silksong menus.
/// </summary>
public static class Registry
{
    public delegate SelectableElement MenuElementGenerator();

    private static readonly List<(string, MenuElementGenerator)> modMenuGenerators = [];

    /// <summary>
    /// Adds a menu element to the Mods sub-menu.
    /// </summary>
    public static void AddModMenu(string name, MenuElementGenerator generator) =>
        modMenuGenerators.Add(
            (
                name ?? throw new ArgumentNullException(nameof(name)),
                generator ?? throw new ArgumentNullException(nameof(generator))
            )
        );

    internal static IEnumerable<SelectableElement> GenerateAllMenuElements()
    {
        List<(string, SelectableElement)> allElements = [];
        foreach (var (name, gen) in modMenuGenerators)
        {
            ExceptionUtil.Try(
                () =>
                {
                    var element = gen();
                    allElements.Add((name, element));
                },
                $"Failed generating '{name}'"
            );
        }

        foreach (var plugin in Chainloader.PluginInfos)
        {
            var instance = plugin.Value.Instance;
            if (instance == null)
                continue;

            ExceptionUtil.Try(
                () =>
                {
                    if (PluginRegistry.GenerateMenuElement(instance, out var name, out var element))
                        allElements.Add((name, element));
                },
                $"Error generating mod menu for {plugin.Value.Metadata.Name}"
            );
        }

        return [.. allElements.OrderBy(p => p.Item1.ToUpper()).Select(p => p.Item2)];
    }
}
