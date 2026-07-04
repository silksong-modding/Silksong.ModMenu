using System;
using System.Collections.Generic;
using System.Globalization;
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

    #region Numbers

    /// <summary>
    /// An ITextModel which parses its input into a signed byte.
    /// </summary>
    public static ParserTextModel<sbyte> ForSignedBytes() =>
        new(sbyte.TryParse, DefaultUnparse<sbyte>);

    /// <summary>
    /// An ITextModel which parses its input into a signed byte clamped between a min and max.
    /// </summary>
    public static ParserTextModel<sbyte> ForSignedBytes(sbyte min, sbyte max)
    {
        var model = ForSignedBytes();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    /// <summary>
    /// An ITextModel which parses its input into a byte.
    /// </summary>
    public static ParserTextModel<byte> ForBytes() => new(byte.TryParse, DefaultUnparse<byte>);

    /// <summary>
    /// An ITextModel which parses its input into a byte clamped between a min and max.
    /// </summary>
    public static ParserTextModel<byte> ForBytes(byte min, byte max)
    {
        var model = ForBytes();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    /// <summary>
    /// An ITextModel which parses its input into a short.
    /// </summary>
    public static ParserTextModel<short> ForShorts() => new(short.TryParse, DefaultUnparse<short>);

    /// <summary>
    /// An ITextModel which parses its input into a short clamped between a min and max.
    /// </summary>
    public static ParserTextModel<short> ForShorts(short min, short max)
    {
        var model = ForShorts();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    /// <summary>
    /// An ITextModel which parses its input into an unsigned short.
    /// </summary>
    public static ParserTextModel<ushort> ForUnsignedShorts() =>
        new(ushort.TryParse, DefaultUnparse<ushort>);

    /// <summary>
    /// An ITextModel which parses its input into an unsigned short clamped between a min and max.
    /// </summary>
    public static ParserTextModel<ushort> ForUnsignedShorts(ushort min, ushort max)
    {
        var model = ForUnsignedShorts();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

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
    /// An ITextModel which parses its input into an unsigned integer.
    /// </summary>
    public static ParserTextModel<uint> ForUnsignedIntegers() =>
        new(uint.TryParse, DefaultUnparse<uint>);

    /// <summary>
    /// An ITextModel which parses its input into an unsigned integer clamped between a min and max.
    /// </summary>
    public static ParserTextModel<uint> ForUnsignedIntegers(uint min, uint max)
    {
        var model = ForUnsignedIntegers();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    /// <summary>
    /// An ITextModel which parses its input into a long.
    /// </summary>
    public static ParserTextModel<long> ForLongs() => new(long.TryParse, DefaultUnparse<long>);

    /// <summary>
    /// An ITextModel which parses its input into a long clamped between a min and max.
    /// </summary>
    public static ParserTextModel<long> ForLongs(long min, long max)
    {
        var model = ForLongs();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    /// <summary>
    /// An ITextModel which parses its input into an unsigned long.
    /// </summary>
    public static ParserTextModel<ulong> ForUnsignedLongs() =>
        new(ulong.TryParse, DefaultUnparse<ulong>);

    /// <summary>
    /// An ITextModel which parses its input into an unsigned long clamped between a min and max.
    /// </summary>
    public static ParserTextModel<ulong> ForUnsignedLongs(ulong min, ulong max)
    {
        var model = ForUnsignedLongs();
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

    /// <summary>
    /// An ITextModel which parses its input into a double.
    /// </summary>
    public static ParserTextModel<double> ForDoubles() =>
        new(double.TryParse, DefaultUnparse<double>);

    /// <summary>
    /// An ITextModel which parses its input into a double clamped between a min and max.
    /// </summary>
    public static ParserTextModel<double> ForDoubles(double min, double max)
    {
        var model = ForDoubles();
        model.ConstraintFn = RangeConstraint(min, max);
        return model;
    }

    #endregion

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

    #region Colors

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

    #endregion
    #region Vectors & Quaternions & Rects

    /// <summary>
    /// An ITextModel which parses comma-delimited pairs of numbers to and from <see cref="Vector2"/>s.
    /// </summary>
    public static ParserTextModel<Vector2> ForVector2() =>
        new(Vector2Parser, Vector2Unparser, INVALID_VECTOR);

    /// <summary>
    /// An ITextModel which parses comma-delimited trios of numbers to and from <see cref="Vector3"/>s.
    /// </summary>
    public static ParserTextModel<Vector3> ForVector3() =>
        new(Vector3Parser, Vector3Unparser, INVALID_VECTOR);

    /// <summary>
    /// An ITextModel which parses comma-delimited quartets of numbers to and from <see cref="Vector4"/>s.
    /// </summary>
    public static ParserTextModel<Vector4> ForVector4() =>
        new(Vector4Parser, Vector4Unparser, INVALID_VECTOR);

    /// <summary>
    /// An ITextModel which parses comma-delimited quartets of numbers to and from <see cref="Quaternion"/>s.
    /// </summary>
    public static ParserTextModel<Quaternion> ForQuaternion() =>
        new(QuaternionParser, QuaternionUnparser, INVALID_QUATERNION);

    /// <summary>
    /// An ITextModel which parses comma-delimited quartets of numbers to and from <see cref="Rect"/>s.
    /// </summary>
    public static ParserTextModel<Rect> ForRect() => new(RectParser, RectUnparser, INVALID_RECT);

    private static readonly Vector4 INVALID_VECTOR = Vector4.positiveInfinity;
    private static readonly Quaternion INVALID_QUATERNION = new(
        float.PositiveInfinity,
        float.PositiveInfinity,
        float.PositiveInfinity,
        float.PositiveInfinity
    );
    private static readonly Rect INVALID_RECT = new(
        float.PositiveInfinity,
        float.PositiveInfinity,
        float.PositiveInfinity,
        float.PositiveInfinity
    );

    private static bool Vector2Parser(string x, out Vector2 v)
    {
        bool success = FloatListParser(x, out float[] c) && c.Length == 2;
        v = success ? new(c[0], c[1]) : INVALID_VECTOR;
        return success;
    }

    private static bool Vector2Unparser(Vector2 v, out string x)
    {
        x = v.ToString("F2", CultureInfo.InvariantCulture.NumberFormat);
        return true;
    }

    private static bool Vector3Parser(string x, out Vector3 v)
    {
        bool success = FloatListParser(x, out float[] c) && c.Length == 3;
        v = success ? new(c[0], c[1], c[2]) : INVALID_VECTOR;
        return success;
    }

    private static bool Vector3Unparser(Vector3 v, out string x)
    {
        x = v.ToString("F2", CultureInfo.InvariantCulture.NumberFormat);
        return true;
    }

    private static bool Vector4Parser(string x, out Vector4 v)
    {
        bool success = FloatListParser(x, out float[] c) && c.Length == 4;
        v = success ? new(c[0], c[1], c[2], c[3]) : INVALID_VECTOR;
        return success;
    }

    private static bool Vector4Unparser(Vector4 v, out string x)
    {
        x = v.ToString("F2", CultureInfo.InvariantCulture.NumberFormat);
        return true;
    }

    private static bool QuaternionParser(string x, out Quaternion q)
    {
        bool success = FloatListParser(x, out float[] c) && c.Length == 4;
        q = success ? new(c[0], c[1], c[2], c[3]) : INVALID_QUATERNION;
        return success;
    }

    private static bool QuaternionUnparser(Quaternion q, out string x)
    {
        x = q.ToString("F3", CultureInfo.InvariantCulture.NumberFormat);
        return true;
    }

    private static bool RectParser(string x, out Rect r)
    {
        bool success = FloatListParser(x, out float[] c) && c.Length == 4;
        r = success ? new(c[0], c[1], c[2], c[3]) : INVALID_RECT;
        return success;
    }

    private static bool RectUnparser(Rect r, out string x)
    {
        const string format = "F2";
        var provider = CultureInfo.InvariantCulture.NumberFormat;
        x = UnityString.Format(
            "({0}, {1}, {2}, {3})",
            r.x.ToString(format, provider),
            r.y.ToString(format, provider),
            r.width.ToString(format, provider),
            r.height.ToString(format, provider)
        );
        return true;
    }

    /// <summary>
    /// Parses strings of the format "(-0.0, -0.0, ... , -0.0)" with or without brackets
    /// into an array of floats.
    /// </summary>
    private static bool FloatListParser(string x, out float[] results)
    {
        x = x.Trim();
        if (x[0] == '(')
            x = x[1..];
        if (x[^1] == ')')
            x = x[..^1];

        string[] rawVals = x.Split(',');
        results = new float[rawVals.Length];

        NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;

        for (int i = 0; i < rawVals.Length; i++)
            if (!float.TryParse(rawVals[i], NumberStyles.Float, format, out results[i]))
                return false;

        return true;
    }

    #endregion
}
