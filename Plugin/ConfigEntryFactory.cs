using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Generate MenuElements for ConfigurationManager entries.
/// </summary>
public static class ConfigEntryFactory
{
    /// <summary>
    /// Generate a selectable menu element for the given config entry, if possible.
    ///
    /// Right now only booleans, enums, and entries with an explicit AcceptableValuesList are supported.
    /// In the future support for numeric and other types may be added.
    /// </summary>
    public static bool GenerateMenuElement(
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out SelectableElement selectableMenuElement
    )
    {
        if (entry is ConfigEntry<bool> boolEntry)
        {
            selectableMenuElement = GenerateMenuElement(boolEntry);
            return true;
        }
        else if (entry.SettingType.IsEnum)
            return GenerateEnumChoiceElement(entry.SettingType, entry, out selectableMenuElement);
        else if (ExtractAcceptableValues(entry, out var array))
            return GenerateChoiceElement(entry, array, out selectableMenuElement);

        // TODO: Numerics, other types?
        selectableMenuElement = default;
        return false;
    }

    private static ChoiceElement<bool> GenerateMenuElement(ConfigEntry<bool> entry)
    {
        ChoiceElement<bool> choice = new(
            entry.LabelName(),
            ChoiceModels.ForBool(),
            entry.DescriptionLine()
        );
        choice.SynchronizeWith(entry);
        return choice;
    }

    private static bool GenerateEnumChoiceElement(
        Type enumType,
        ConfigEntryBase entry,
        [MaybeNullWhen(false)] out SelectableElement selectableMenuElement
    )
    {
        selectableMenuElement = default;
        if (!ChoiceModels.TryForEnum<object>(enumType, out var model))
            return false;

        ChoiceElement<object> choice = new(entry.LabelName(), model, entry.DescriptionLine());
        choice.SynchronizeRawWith(entry);

        selectableMenuElement = choice;
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

    private static bool GenerateChoiceElement(
        ConfigEntryBase entry,
        Array values,
        [MaybeNullWhen(false)] out SelectableElement selectableMenuElement
    )
    {
        if (values.Length == 0)
        {
            selectableMenuElement = default;
            return false;
        }

        ChoiceElement<object> choice = new(
            entry.LabelName(),
            ChoiceModels.ForValues([.. values]),
            entry.DescriptionLine()
        );
        choice.SynchronizeRawWith(entry);

        selectableMenuElement = choice;
        return true;
    }

    /// <summary>
    /// Synchronize the menu element model and config setting so that if/when one changes, so does the other.
    /// Callers should prefer SynchronizeWith instead of WithRaw when possible, for type safety.
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

    private static string LabelName(this ConfigEntryBase self) =>
        self.Definition.Key.UnCamelCase().Truncate(50);

    private static string DescriptionLine(this ConfigEntryBase self) =>
        self.Description.Description.FirstLine(150);
}
