using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenu.Plugin;

public delegate bool PluginHandler(
    IModMenuInterface plugin,
    [MaybeNullWhen(false)] out SelectableElement menuElement
);

/// <summary>
/// Registry of all IModMenuInterface extensions, used for generating mod menus from plugins.
/// </summary>
public static class PluginRegistry
{
    private static readonly List<PluginHandler> registeredHandlers =
    [
        HandleType<IModMenuCustomElement>(GenerateCustomElement),
        HandleType<IModMenuCustomMenu>(GenerateCustomMenu),
        HandleType<IModMenuToggle>(GenerateToggle),
    ];

    /// <summary>
    /// Register a custom plugin interface handler.
    /// </summary>
    public static void AddHandler(PluginHandler handler) => registeredHandlers.Add(handler);

    /// <summary>
    /// Make a PluginHandler for a specific sub-type.
    /// </summary>
    public static PluginHandler HandleType<T>(Func<T, SelectableElement> generator)
        where T : IModMenuInterface
    {
        bool Handler(
            IModMenuInterface plugin,
            [MaybeNullWhen(false)] out SelectableElement menuElement
        )
        {
            if (plugin is T typed)
            {
                menuElement = generator(typed);
                return true;
            }

            menuElement = default;
            return false;
        }

        return Handler;
    }

    internal static bool GenerateMenuElement(
        BaseUnityPlugin plugin,
        out string name,
        [MaybeNullWhen(false)] out SelectableElement menuElement
    )
    {
        if (plugin.GetType().IgnoreForModMenu())
        {
            name = "";
            menuElement = default;
            return false;
        }

        name = plugin.Info.Metadata.Name.UnCamelCase();
        if (plugin is IModMenuInterface typed)
        {
            name = typed.ModMenuName();
            foreach (var handler in registeredHandlers)
            {
                if (handler(typed, out menuElement))
                    return true;
            }
        }

        return GenerateDefaultMenuElement(name, plugin, out menuElement);
    }

    private static bool GenerateDefaultMenuElement(
        string name,
        BaseUnityPlugin plugin,
        [MaybeNullWhen(false)] out SelectableElement menuElement
    )
    {
        List<SelectableElement> elements = [];
        foreach (var entry in plugin.Config)
        {
            if (ConfigEntryFactory.GenerateMenuElement(entry.Value, out var element))
                elements.Add(element);
        }

        if (elements.Count == 0)
        {
            menuElement = default;
            return false;
        }

        PaginatedMenuScreen menu = new(name);
        menu.Add(elements);

        menuElement = new TextButton(name) { OnSubmit = () => MenuScreenNavigation.Show(menu) };
        return true;
    }

    private static SelectableElement GenerateCustomElement(IModMenuCustomElement plugin) =>
        plugin.BuildCustomElement();

    private static SelectableElement GenerateCustomMenu(IModMenuCustomMenu plugin)
    {
        var menu = plugin.BuildCustomMenu();
        return new TextButton(plugin.ModMenuName())
        {
            OnSubmit = () => MenuScreenNavigation.Show(menu),
        };
    }

    private static SelectableElement GenerateToggle(IModMenuToggle plugin)
    {
        ChoiceElement<bool> element = new(
            plugin.ModMenuName(),
            ChoiceModels.ForBool("Disabled", "Enabled"),
            plugin.ModMenuDescription()
        )
        {
            Value = plugin.ModMenuGetEnabled(),
        };
        element.OnValueChanged += plugin.ModMenuSetEnabled;

        return element;
    }
}
