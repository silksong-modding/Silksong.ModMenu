using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Silksong.ModMenu.Models;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Utility for navigating enums through reflection, retrieving a map from values -> string representations.
/// Respects ModMenu attributes.
/// </summary>
internal static class EnumStrings
{
    private static readonly Dictionary<Type, ReadOnlyDictionary<object, string>> enumStrings = [];

    internal static ReadOnlyDictionary<object, string> ForType(Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException($"{enumType.Name} is not an Enumeration");
        if (enumStrings.TryGetValue(enumType, out var dict))
            return dict;

        Dictionary<object, List<FieldInfo>> fieldInfos = [];
        foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var rawValue = field.GetRawConstantValue();
            if (fieldInfos.TryGetValue(rawValue, out var list))
                list.Add(field);
            else
                fieldInfos.Add(rawValue, [field]);
        }

        Dictionary<object, string> names = [];
        foreach (var value in enumType.GetEnumValues())
        {
            var rawValue = Convert.ChangeType(value, enumType.GetEnumUnderlyingType());
            if (!fieldInfos.TryGetValue(rawValue, out var list))
                continue;

            bool ignore = false;
            string? fieldName = null;
            string? customName = null;
            foreach (var field in list)
            {
                if (field.IgnoreForModMenu())
                {
                    ignore = true;
                    break;
                }

                customName ??= field.GetCustomAttribute<ModMenuName>()?.CustomName;
                if (customName == null)
                    fieldName ??= StringUtil.UnCamelCase(field.Name);
            }

            if (ignore)
                continue;

            names[value] = customName ?? fieldName!;
        }

        ReadOnlyDictionary<object, string> readOnlyDict = new(names);
        enumStrings.Add(enumType, readOnlyDict);
        return readOnlyDict;
    }
}
