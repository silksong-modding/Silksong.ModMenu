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
        var model = ChoiceModels.ForBool();
        Synchronize(model, entry);

        return new ChoiceElement<bool>(entry.LabelName(), model, entry.DescriptionLine());
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

        selectableMenuElement = new ChoiceElement<object>(
            entry.LabelName(),
            model.SynchronizedRawTo(entry),
            entry.DescriptionLine()
        );
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

        selectableMenuElement = new ChoiceElement<object>(
            entry.LabelName(),
            ChoiceModels.ForValues([.. values]).SynchronizedRawTo(entry),
            entry.DescriptionLine()
        );
        return true;
    }

    /// <summary>
    /// Synchronize the menu element model and config setting so that if/when one changes, so does the other.
    /// </summary>
    public static void SynchronizeRaw(IBaseValueModel model, ConfigEntryBase entry)
    {
        model.SetRawValue(entry.BoxedValue);

        model.OnRawValueChanged += o => entry.BoxedValue = o;
        entry.ConfigFile.SettingChanged += (_, args) =>
        {
            if (args.ChangedSetting == entry)
                model.SetRawValue(args.ChangedSetting.BoxedValue);
        };
    }

    /// <summary>
    /// Extension helper for easier construction of synchronized menu elements.
    /// </summary>
    public static M SynchronizedRawTo<M>(this M model, ConfigEntryBase entry)
        where M : IBaseValueModel
    {
        SynchronizeRaw(model, entry);
        return model;
    }

    /// <summary>
    /// Synchronize the menu element model and config setting so that if/when one changes, so does the other.
    /// </summary>
    public static void Synchronize<T>(IValueModel<T> model, ConfigEntry<T> entry)
    {
        model.SetValue(entry.Value);

        model.OnValueChanged += v => entry.Value = v;
        entry.SettingChanged += (_, args) =>
            model.SetValue((T)((SettingChangedEventArgs)args).ChangedSetting.BoxedValue);
    }

    /// <summary>
    /// Extension helper for easier construction of synchronized menu elements.
    /// </summary>
    public static M SynchronizedTo<T, M>(this M model, ConfigEntry<T> entry)
        where M : IValueModel<T>
    {
        Synchronize(model, entry);
        return model;
    }

    private static string LabelName(this ConfigEntryBase self) =>
        self.Definition.Key.UnCamelCase().Truncate(50);

    private static string DescriptionLine(this ConfigEntryBase self) =>
        self.Description.Description.FirstLine(150);
}
