using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;
using UnityEngine;

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
        GenerateKeyCodeElement,
        GenerateEnumChoiceElement,
        GenerateAcceptableValuesChoiceElement,
        GenerateBoolElement,
        GenerateSByteElement,
        GenerateByteElement,
        GenerateShortElement,
        GenerateUShortElement,
        GenerateIntElement,
        GenerateUIntElement,
        GenerateLongElement,
        GenerateULongElement,
        GenerateFloatElement,
        GenerateDoubleElement,
        GenerateStringElement,
        GenerateColorElement,
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
    /// If true, organize elements heirarchically by the period-delimeted names of their config definitions.
    /// </summary>
    public bool GenerateSubgroups = false;

    /// <summary>
    /// The minimum number of elements required to generate a subgroup. Has no effect if GenerateSubgroups is false.
    /// </summary>
    public int MinSubgroupSize = 2;

    private static IEnumerable<LocalizedText> SubgroupsFromConfig(ConfigDefinition config) =>
        config.Section.Split('.').Concat(config.Key.Split('.')).Select(LocalizedText.Raw);

    /// <summary>
    /// The hierarchical subgroup names to use for each config entry.
    /// </summary>
    protected virtual IEnumerable<LocalizedText> GetSubgroupNames(
        ConfigEntryBase config,
        MenuElement menuElement
    )
    {
        var subgroups = config.Description.Tags.OfType<ConfigEntrySubgroup>().FirstOrDefault();
        return subgroups?.Subgroups
            ?? (GenerateSubgroups ? SubgroupsFromConfig(config.Definition) : []);
    }

    private static void FindFirstNonEmptyChild(
        ref TreeNode<LocalizedText, ElementTreeNode> tree,
        List<LocalizedText> keys
    )
    {
        while (tree.Value.TotalElements <= 1 && tree.Subtrees.Count == 1)
        {
            var (key, subtree) = tree.Subtrees.First();
            keys.Add(key);
            tree = subtree;
        }
    }

    private List<(string, MenuElement)> BuildSubtreeElements(
        LocalizedText menuName,
        List<LocalizedText> subpageNames,
        TreeNode<LocalizedText, ElementTreeNode> tree
    )
    {
        // Return elements directly if there is not enough of them.
        if (tree.Value.TotalElements < MinSubgroupSize)
        {
            List<(string, MenuElement)> list = [];
            tree.ForEachPostfix((keys, t) => list.AddRange(t.Value.Elements));
            list.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return list;
        }

        // Otherwise, build a sub-page and return a button that navigates to it.
        var name = string.Join(".", subpageNames.Select(n => n.Canonical));
        var screen = BuildSubtreeScreen(menuName, subpageNames, tree);
        TextButton button = new(subpageNames.Last())
        {
            OnSubmit = () => MenuScreenNavigation.Show(screen),
        };
        return [(name, button)];
    }

    private AbstractMenuScreen BuildSubtreeScreen(
        LocalizedText menuName,
        List<LocalizedText> subpageNames,
        TreeNode<LocalizedText, ElementTreeNode> tree
    )
    {
        List<(string path, MenuElement element)> elements = [.. tree.Value.Elements];

        int origSize = subpageNames.Count;
        foreach (var entry in tree.Subtrees)
        {
            var subtree = entry.Value;
            subpageNames.Add(entry.Key);
            FindFirstNonEmptyChild(ref subtree, subpageNames);
            elements.AddRange(BuildSubtreeElements(menuName, subpageNames, subtree));
            subpageNames.RemoveRange(origSize, subpageNames.Count - origSize);
        }

        return ArrangeScreen(
            elements.OrderBy(e => e.path).ToList(),
            subpageNames.LastOrDefault() ?? menuName
        );
    }

    /// <summary>
    /// Given a list of menu elements, create a menu screen for those elements.
    ///
    /// This function is used by the default implementation of
    /// <see cref="GenerateEntryButton(LocalizedText, BaseUnityPlugin, out SelectableElement)"/>
    /// to build each subpage.
    /// </summary>
    /// <param name="elements">Pairs (path to element, element) to arrange.</param>
    /// <param name="menuName">The title of the menu.</param>
    /// <returns>A menu screen containing those elements.</returns>
    protected virtual AbstractMenuScreen ArrangeScreen(
        List<(string path, MenuElement element)> elements,
        LocalizedText menuName
    )
    {
        PaginatedMenuScreenBuilder builder = new(menuName);
        builder.AddRange(elements.Select(e => e.element));
        return builder.Build();
    }

    /// <summary>
    /// Generate a button for this plugin which opens a sub-menu for its ConfigFile.
    /// </summary>
    public virtual bool GenerateEntryButton(
        LocalizedText name,
        BaseUnityPlugin plugin,
        [MaybeNullWhen(false)] out SelectableElement selectableElement
    ) => GenerateEntryButton(name, plugin.Config, out _, out selectableElement);

    /// <summary>
    /// Generate a button which opens a sub-menu for this ConfigFile.
    /// </summary>
    public virtual bool GenerateEntryButton(
        LocalizedText name,
        ConfigFile config,
        [MaybeNullWhen(false)] out AbstractMenuScreen menuScreen,
        [MaybeNullWhen(false)] out SelectableElement selectableElement
    )
    {
        TreeNode<LocalizedText, ElementTreeNode> elementsTree = new();
        foreach (var entry in config.OrderBy(e => e.Key.Key))
        {
            if (GenerateMenuElement(entry.Value, out var element))
            {
                var subgroupNames = GetSubgroupNames(entry.Value, element);
                elementsTree[subgroupNames].Value.Elements.Add((entry.Key.Key, element));
            }
        }

        // Count the total number of elements in each subtree.
        // The number of elements counted for each subtree is 1 if it gets its own menu button, N otherwise.
        elementsTree.ForEachPostfix(
            (_, tree) =>
                tree.Value.TotalElements =
                    tree.Value.Elements.Count
                    + tree.Subtrees.Values.Select(t =>
                            t.Value.TotalElements >= MinSubgroupSize ? 1 : t.Value.TotalElements
                        )
                        .Sum()
        );
        if (elementsTree.Value.TotalElements == 0)
        {
            menuScreen = default;
            selectableElement = default;
            return false;
        }

        // Skip past any universal prefix.
        List<LocalizedText> subpageNames = [];
        FindFirstNonEmptyChild(ref elementsTree, subpageNames);

        AbstractMenuScreen menu = BuildSubtreeScreen(name, subpageNames, elementsTree);
        menuScreen = menu;
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
    public virtual bool GenerateMenuElement(
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

    /// <summary>
    /// Generate a menu element for a config with a custom generator.
    /// </summary>
    public static bool GenerateCustomElement(
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

    /// <summary>
    /// Generate a menu element for a key bind.
    /// </summary>
    public static bool GenerateKeyCodeElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<KeyCode> keyCodeEntry)
        {
            menuElement = default;
            return false;
        }

        KeyBindElement element = new(entry.LabelName());
        element.SynchronizeWith(keyCodeEntry);

        menuElement = element;
        return true;
    }

    /// <summary>
    /// Generate a menu element for a config entry on an enum type.
    /// </summary>
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

    /// <summary>
    /// Generate a menu element for a config setting with an explicit list of acceptable values.
    /// </summary>
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

    /// <summary>
    /// Generate a menu element for a config setting with a boolean value.
    /// </summary>
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

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged sbyte value.
    /// </summary>
    public static bool GenerateSByteElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<sbyte> sByteEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<sbyte> range)
                ? TextModels.ForSignedBytes(range.MinValue, range.MaxValue)
                : TextModels.ForSignedBytes();

        TextInput<sbyte> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(sByteEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged byte value.
    /// </summary>
    public static bool GenerateByteElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<byte> byteEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<byte> range)
                ? TextModels.ForBytes(range.MinValue, range.MaxValue)
                : TextModels.ForBytes();

        TextInput<byte> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(byteEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged short value.
    /// </summary>
    public static bool GenerateShortElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<short> shortEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<short> range)
                ? TextModels.ForShorts(range.MinValue, range.MaxValue)
                : TextModels.ForShorts();

        TextInput<short> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(shortEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged unsigned short value.
    /// </summary>
    public static bool GenerateUShortElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<ushort> uShortEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<ushort> range)
                ? TextModels.ForUnsignedShorts(range.MinValue, range.MaxValue)
                : TextModels.ForUnsignedShorts();

        TextInput<ushort> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(uShortEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged int value.
    /// </summary>
    public static bool GenerateIntElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<int> intEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<int> range)
                ? TextModels.ForIntegers(range.MinValue, range.MaxValue)
                : TextModels.ForIntegers();

        TextInput<int> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(intEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged unsigned int value.
    /// </summary>
    public static bool GenerateUIntElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<uint> uIntEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<uint> range)
                ? TextModels.ForUnsignedIntegers(range.MinValue, range.MaxValue)
                : TextModels.ForUnsignedIntegers();

        TextInput<uint> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(uIntEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged long value.
    /// </summary>
    public static bool GenerateLongElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<long> longEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<long> range)
                ? TextModels.ForLongs(range.MinValue, range.MaxValue)
                : TextModels.ForLongs();

        TextInput<long> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(longEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged unsigned long value.
    /// </summary>
    public static bool GenerateULongElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<ulong> uLongEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<ulong> range)
                ? TextModels.ForUnsignedLongs(range.MinValue, range.MaxValue)
                : TextModels.ForUnsignedLongs();

        TextInput<ulong> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(uLongEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged float value.
    /// </summary>
    public static bool GenerateFloatElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<float> floatEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<float> range)
                ? TextModels.ForFloats(range.MinValue, range.MaxValue)
                : TextModels.ForFloats();

        TextInput<float> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(floatEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generates a menu element for a config setting with a free or ranged double value.
    /// </summary>
    public static bool GenerateDoubleElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<double> doubleEntry)
        {
            menuElement = default;
            return false;
        }

        var acceptableValues = entry.Description.AcceptableValues;
        var model =
            (acceptableValues is AcceptableValueRange<double> range)
                ? TextModels.ForDoubles(range.MinValue, range.MaxValue)
                : TextModels.ForDoubles();

        TextInput<double> text = new(entry.LabelName(), model, entry.DescriptionLine());
        text.SynchronizeWith(doubleEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generate a text element for an arbitrary string.
    /// </summary>
    public static bool GenerateStringElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<string> stringEntry)
        {
            menuElement = default;
            return false;
        }

        TextInput<string> text = new(
            entry.LabelName(),
            TextModels.ForStrings(),
            entry.DescriptionLine()
        );
        text.SynchronizeWith(stringEntry);

        menuElement = text;
        return true;
    }

    /// <summary>
    /// Generate a text element for a color.
    /// </summary>
    public static bool GenerateColorElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out MenuElement menuElement
    )
    {
        if (entry is not ConfigEntry<Color> colorEntry)
        {
            menuElement = default;
            return false;
        }

        ColorInput color = new(entry.LabelName(), entry.DescriptionLine())
        {
            Format =
                (entry.Description.AcceptableValues is RGBColorValues)
                    ? ColorInput.InputFormat.RGB
                    : ColorInput.InputFormat.RGBA,
        };
        color.SynchronizeWith(colorEntry);

        menuElement = color;
        return true;
    }

    private record ElementTreeNode
    {
        public readonly List<(string path, MenuElement element)> Elements = [];
        public int TotalElements;
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

    /// <summary>
    /// Short, stylized text label for a config entry.
    /// </summary>
    public static string LabelName(this ConfigEntryBase self) =>
        self.Definition.Key.UnCamelCase().Truncate(50);

    /// <summary>
    /// Truncated description for a config entry.
    /// </summary>
    public static string DescriptionLine(this ConfigEntryBase self) =>
        self.Description.Description.FirstLine(150);
}
