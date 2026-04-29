using System;
using System.Collections.Generic;
using System.Linq;
using Silksong.ModMenu.Internal;
using UnityEngine;

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
    /// An ITextModel which parses its input into into <typeparamref name="T"/> values clamped between a min and max.
    /// </summary>
    /// <remarks>
    /// Only works with numeric value types such as <see langword="int"/>, <see langword="float"/>, etc.
    /// </remarks>
    public static ParserTextModel<T> ForNumbers<T>(T min, T max)
        where T : struct, IComparable<T>
    {
        var model = ForNumbers<T>();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    /// <summary>
    /// An ITextModel which parses its input into <typeparamref name="T"/> values.
    /// </summary>
    /// <remarks>
    /// Only works with numeric value types such as <see langword="int"/>, <see langword="float"/>, etc.
    /// </remarks>
    public static ParserTextModel<T> ForNumbers<T>()
        where T : struct, IComparable<T>
    {
        ParserTextModel<T>.Parse parser = default(T) switch
        {
            byte => static (text, out value) =>
            {
                bool res = byte.TryParse(text, out byte parsed);
                value = (T)(object)parsed;
                return res;
            },
            sbyte => static (text, out value) =>
            {
                bool res = sbyte.TryParse(text, out sbyte parsed);
                value = (T)(object)parsed;
                return res;
            },
            short => static (text, out value) =>
            {
                bool res = short.TryParse(text, out short parsed);
                value = (T)(object)parsed;
                return res;
            },
            ushort => static (text, out value) =>
            {
                bool res = ushort.TryParse(text, out ushort parsed);
                value = (T)(object)parsed;
                return res;
            },
            int => static (text, out value) =>
            {
                bool res = int.TryParse(text, out int parsed);
                value = (T)(object)parsed;
                return res;
            },
            uint => static (text, out value) =>
            {
                bool res = uint.TryParse(text, out uint parsed);
                value = (T)(object)parsed;
                return res;
            },
            long => static (text, out value) =>
            {
                bool res = long.TryParse(text, out long parsed);
                value = (T)(object)parsed;
                return res;
            },
            ulong => static (text, out value) =>
            {
                bool res = ulong.TryParse(text, out ulong parsed);
                value = (T)(object)parsed;
                return res;
            },
            float => static (text, out value) =>
            {
                bool res = float.TryParse(text, out float parsed);
                value = (T)(object)parsed;
                return res;
            },
            double => static (text, out value) =>
            {
                bool res = double.TryParse(text, out double parsed);
                value = (T)(object)parsed;
                return res;
            },
            decimal => static (text, out value) =>
            {
                bool res = decimal.TryParse(text, out decimal parsed);
                value = (T)(object)parsed;
                return res;
            },
            _ => throw new ArgumentException($"{typeof(T)} is not a numeric value type."),
        };
        return new(parser, DefaultUnparse);
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

    /// <summary>
    /// An ITextModel which parses 3, 6, or 8 character hex strings to and from <see cref="Color"/>s.
    /// </summary>
    public static ParserTextModel<Color> ForHexColors() =>
        new(HexParser, HexUnparser, INVALID_COLOR);

    private static readonly Color INVALID_COLOR = new(-1, -1, -1, -1);

    private static bool HexParser(string x, out Color c)
    {
        if (x.Length == 8 && x[6..8].TryParseHex(out byte a)) { }
        else
            a = byte.MaxValue;

        if (
            (x.Length == 6 || x.Length == 8)
            && x[0..2].TryParseHex(out byte r)
            && x[2..4].TryParseHex(out byte g)
            && x[4..6].TryParseHex(out byte b)
        )
        {
            c = new Color32(r, g, b, a);
            return true;
        }
        else if (
            x.Length == 3
            && $"{x[0]}{x[0]}".TryParseHex(out r)
            && $"{x[1]}{x[1]}".TryParseHex(out g)
            && $"{x[2]}{x[2]}".TryParseHex(out b)
        )
        {
            c = new Color32(r, g, b, a);
            return true;
        }

        c = INVALID_COLOR;
        return false;
    }

    private static bool HexUnparser(Color c, out string x)
    {
        if (!Enumerable.Range(0, 3).Any(i => c[i] < 0 || 1 < c[i]))
        {
            Color32 c32 = c;
            x = $"{c32.r:X2}{c32.g:X2}{c32.b:X2}{c32.a:X2}";
            return true;
        }
        x = "###";
        return false;
    }
}
