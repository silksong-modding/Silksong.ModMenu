using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Configuration;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Class with utility methods for creating a MenuElementGenerator.
/// </summary>
public static class MenuElementGenerators
{
    /// <summary>
    /// Returns a Generator that creates a choice element with a description below the option value.
    ///
    /// The description will be taken from the <see cref="DescriptionAttribute"/> attributes on the enum members.
    /// </summary>
    /// <param name="includeSettingDescription">If true, will also include the default description below the setting name.</param>
    public static ConfigEntryFactory.MenuElementGenerator CreateRightDescGenerator(
        bool includeSettingDescription = true
    )
    {
        bool gen(ConfigEntryBase entry, [MaybeNullWhen(false)] out MenuElement element)
        {
            if (!entry.SettingType.IsEnum)
            {
                element = null;
                return false;
            }

            if (
                !ChoiceModels.TryForEnum<object>(
                    entry.SettingType,
                    out ListChoiceModel<object>? model
                )
            )
            {
                element = default;
                return false;
            }

            Dictionary<object, string> descriptionLookup = model.Values.ToDictionary(
                t => t,
                t => string.Empty
            );
            bool anyDesc = false;
            foreach (object member in model.Values)
            {
                FieldInfo enumField = entry.SettingType.GetField(
                    member.ToString(),
                    BindingFlags.Static | BindingFlags.Public
                );
                DescriptionAttribute? desc = enumField.GetCustomAttribute<DescriptionAttribute>();
                if (desc is not null)
                {
                    descriptionLookup[member] = desc.Description;
                    anyDesc = true;
                }
            }

            if (!anyDesc)
            {
                element = default;
                return false;
            }

            string leftDesc = includeSettingDescription ? entry.DescriptionLine() : string.Empty;

            RightDescriptionChoiceElement<object> typedElement = new(
                entry.LabelName(),
                model,
                leftDesc,
                t => descriptionLookup[t]
            );
            typedElement.SynchronizeRawWith(entry);

            element = typedElement;
            return true;
        }

        return gen;
    }
}
