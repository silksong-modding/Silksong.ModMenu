using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Configuration;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Factory to generate a menu screen from a config file. Clients can instantiate this class directly, or subclass it, to inject their own behaviours.
/// </summary>
public class ConfigEntryFactory
{
    /// <summary>
    /// Function to generate a menu element for a specific config setting.
    /// Can also be used as an attribute on a config setting to define its own behaviour explicitly.
    /// </summary>
    public delegate bool MenuElementGenerator(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    );

    private static readonly List<MenuElementGenerator> defaultGenerators =
    [
        GenerateCustomElement,
        GenerateBoolElement,
        GenerateEnumChoiceElement,
        GenerateAcceptableValuesChoiceElement,
    ];

    /// <summary>
    /// Register a new ConfigEntryBase handler which runs after all the built-in defaults.
    /// </summary>
    public static void AddDefaultGenerator(MenuElementGenerator generator) =>
        defaultGenerators.Add(generator ?? throw new ArgumentNullException(nameof(generator)));

    /// <summary>
    /// Ordered list of generators to use on individual config entries. Callers can modify this for custom behavior.
    /// </summary>
    public readonly List<MenuElementGenerator> Generators = [.. defaultGenerators];

    /// <summary>
    /// Generate a button for this plugin which opens a sub-menu for its ConfigFile.
    /// </summary>
    public virtual bool GenerateEntryButton(
        string name,
        BaseUnityPlugin plugin,
        [MaybeNullWhen(false)] out SelectableElement selectableElement
    )
    {
        List<MenuElement> elements = [];
        foreach (var entry in plugin.Config)
        {
            if (GenerateMenuElement(entry.Value, out var element))
                elements.Add(element);
        }

        if (elements.Count == 0)
        {
            selectableElement = default;
            return false;
        }

        PaginatedMenuScreenBuilder builder = new(name);
        builder.AddRange(elements);
        var menu = builder.Build();

        selectableElement = new TextButton(name)
        {
            OnSubmit = () => MenuScreenNavigation.Show(menu),
        };
        return true;
    }

    /// <summary>
    /// Generate a selectable menu element for the given config entry, if possible.
    ///
    /// Right now only booleans, enums, and entries with an explicit AcceptableValuesList are supported.
    /// In the future support for numeric and other types may be added.
    /// </summary>
    protected virtual bool GenerateMenuElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        foreach (var generator in Generators)
        {
            if (generator(entry, out menuElement))
                return true;
        }

        menuElement = default;
        return false;
    }

    private static bool GenerateCustomElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        foreach (var tag in entry.Description.Tags)
        {
            if (tag is MenuElementGenerator generator && generator(entry, out menuElement))
                return true;
        }

        menuElement = default;
        return false;
    }

    public static bool GenerateBoolElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<bool> boolEntry)
        {
            menuElement = default;
            return false;
        }

        ChoiceElement<bool> choice = new(
            boolEntry.LabelName(),
            ChoiceModels.ForBool(),
            boolEntry.DescriptionLine()
        );
        choice.SynchronizeWith(boolEntry);

        menuElement = choice;
        return true;
    }

    public static bool GenerateEnumChoiceElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        menuElement = default;
        if (!entry.SettingType.IsEnum)
            return false;
        if (!ChoiceModels.TryForEnum<object>(entry.SettingType, out var model))
            return false;

        ChoiceElement<object> choice = new(entry.LabelName(), model, entry.DescriptionLine());
        choice.SynchronizeRawWith(entry);

        menuElement = choice;
        return true;
    }

    private static bool ExtractAcceptableValues(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out Array values
    )
    {
        values = default;

        var acceptableValues = entry.Description.AcceptableValues;
        if (acceptableValues == null)
            return false;

        var type = typeof(AcceptableValueList<>).MakeGenericType(acceptableValues.ValueType);
        if (type.IsInstanceOfType(acceptableValues))
        {
            var prop = type.GetProperty(
                "AcceptableValues",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            values = (prop.GetValue(acceptableValues) as Array)!;
            return true;
        }

        values = default;
        return false;
    }

    public static bool GenerateAcceptableValuesChoiceElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        menuElement = default;

        if (!ExtractAcceptableValues(entry, out var values) || values.Length == 0)
            return false;

        ChoiceElement<object> choice = new(
            entry.LabelName(),
            ChoiceModels.ForValues([.. values]),
            entry.DescriptionLine()
        );
        choice.SynchronizeRawWith(entry);

        menuElement = choice;
        return true;
    }
}

/// <summary>
/// Helpful extensions within a ConfigEntryFactory.
/// </summary>
public static class ConfigEntryFactoryExtensions
{
    /// <summary>
    /// Synchronize the menu element model and config setting so that if/when one changes, so does the other.
    /// Callers should prefer SynchronizeWith instead of SynchronizeRawWith when possible, for type safety.
    /// </summary>
    public static void SynchronizeRawWith(
        this BaseSelectableValueElement element,
        ConfigEntryBase entry
    )
    {
        var model = element.RawModel;
        model.SetRawValue(entry.BoxedValue);
        model.OnRawValueChanged += o => entry.BoxedValue = o;

        void handler(object _, SettingChangedEventArgs args)
        {
            if (args.ChangedSetting == entry)
                model.SetRawValue(args.ChangedSetting.BoxedValue);
        }
        entry.ConfigFile.SettingChanged += handler;
        element.OnDispose += () => entry.ConfigFile.SettingChanged -= handler;
    }

    /// <summary>
    /// Synchronize the menu element model and config setting so that if/when one changes, so does the other.
    /// </summary>
    public static void SynchronizeWith<T>(
        this SelectableValueElement<T> element,
        ConfigEntry<T> entry
    )
    {
        var model = element.Model;
        model.SetValue(entry.Value);
        model.OnValueChanged += v => entry.Value = v;

        void handler(object _, EventArgs args) =>
            model.SetValue((T)((SettingChangedEventArgs)args).ChangedSetting.BoxedValue);
        entry.SettingChanged += handler;
        element.OnDispose += () => entry.SettingChanged -= handler;
    }

    public static string LabelName(this ConfigEntryBase self) =>
        self.Definition.Key.UnCamelCase().Truncate(50);

    public static string DescriptionLine(this ConfigEntryBase self) =>
        self.Description.Description.FirstLine(150);
}
