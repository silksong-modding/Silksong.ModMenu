using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Silksong.ModMenu.Internal;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A collection of helper functions for common types of IChoiceModels.
/// </summary>
public static class ChoiceModels
{
    /// <summary>
    /// A simple boolean model with custom names for 'false' and 'true' in that order.
    /// </summary>
    public static ListChoiceModel<bool> ForBool(string falseName, string trueName) =>
        ForNamedValues([(false, falseName), (true, trueName)]);

    /// <summary>
    /// A default boolean model with the names 'False' and 'True'.
    /// </summary>
    public static ListChoiceModel<bool> ForBool() => ForBool("False", "True");

    /// <summary>
    /// A model for all values of the given enum type. Can be reified to T=object.
    /// Returns false if the enum has no usable values, since empty choice models are not allowed.
    /// </summary>
    public static bool TryForEnum<T>(
        Type enumType,
        [MaybeNullWhen(false)] out ListChoiceModel<T> model
    )
    {
        var dict = EnumUtil.StringsForType(enumType);
        HashSet<object> dupes = [];
        List<(T, string)> values = [];
        foreach (var obj in enumType.GetEnumValues())
        {
            if (!dict.TryGetValue(obj, out var name))
                continue;
            if (!dupes.Add(obj))
                continue;

            T value = (T)obj;
            values.Add((value, name));
        }

        if (values.Count == 0)
        {
            model = default;
            return false;
        }
        else
        {
            model = ForNamedValues(values);
            return true;
        }
    }

    /// <summary>
    /// A model for all values of the given enum type.
    /// </summary>
    public static bool TryForEnum<T>([MaybeNullWhen(false)] out ListChoiceModel<T> model)
        where T : Enum => TryForEnum(typeof(T), out model);

    /// <summary>
    /// Generate a ChoiceModel for the given enum type, or throw if the enum is empty.
    /// </summary>
    public static ListChoiceModel<T> ForEnum<T>()
        where T : Enum =>
        TryForEnum<T>(out var model)
            ? model
            : throw new ArgumentException($"Enum {typeof(T).FullName} is empty.");

    /// <summary>
    /// A model for an arbitrary list of values, with explicitly provided display names.
    /// </summary>
    public static ListChoiceModel<T> ForNamedValues<T>(IEnumerable<(T, string)> values)
    {
        List<T> items = [];
        List<string> names = [];
        foreach (var (item, name) in values)
        {
            items.Add(item);
            names.Add(name);
        }

        return new(items) { DisplayFn = (idx, _) => names[idx] };
    }

    /// <summary>
    /// A model for an arbitrary list of values, using the default ToString() display name.
    /// </summary>
    public static ListChoiceModel<T> ForValues<T>(List<T> values) => new(values);
}
