using System;
using System.Collections.Generic;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A collection of helper methods for ITextModels.
/// </summary>
public static class TextModels
{
    /// <summary>
    /// A transparent ITextModel that uses the text value as the domain value.
    /// </summary>
    public static ParserTextModel<string> ForStrings() =>
        new(DefaultUnparse<string>, DefaultUnparse<string>);

    /// <summary>
    /// An ITextModel which parses its input into an integer.
    /// </summary>
    public static ParserTextModel<int> ForIntegers() => new(int.TryParse, DefaultUnparse<int>);

    /// <summary>
    /// An ITextModel which parses its input into an integer clamped between a min and max.
    /// </summary>
    public static ParserTextModel<int> ForIntegers(int min, int max)
    {
        var model = ForIntegers();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    /// <summary>
    /// An ITextModel which parses its input into a float.
    /// </summary>
    public static ParserTextModel<float> ForFloats() => new(float.TryParse, DefaultUnparse<float>);

    /// <summary>
    /// An ITextModel which parses its input into a float clamped between a min and max.
    /// </summary>
    public static ParserTextModel<float> ForFloats(float min, float max)
    {
        var model = ForFloats();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    private static bool DefaultUnparse<T>(T value, out string text)
    {
        text = $"{value}";
        return true;
    }

    private static Func<T, bool> RangeConstraint<T>(T min, T max)
        where T : IComparable<T>
    {
        bool Check(T value)
        {
            if (Comparer<T>.Default.Compare(value, min) < 0)
                return false;
            if (Comparer<T>.Default.Compare(value, max) > 0)
                return false;
            return true;
        }

        return Check;
    }
}
