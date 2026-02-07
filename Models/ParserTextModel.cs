using System;
using System.Collections.Generic;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Models;

/// <summary>
/// A delegate-based text model which converts text to and from the domain type.
/// Bijection is not necessary, but conversions should stabilize within two rounds to avoid bizarre behaviors.
///
/// Text takes precedence over domain values, to allow input elements to temporarily have invalid states. While invalid text is stored, the domain value is reported as the supplied `defaultValue`. Consumers should always check `IsTextValid` before consuming the domain value.
/// </summary>
public class ParserTextModel<T> : ITextModel<T>
{
    /// <summary>
    /// Delegate for parsing arbitrary text into a domain value.
    /// </summary>
    public delegate bool Parse(string text, out T value);

    /// <summary>
    /// Delegate for converting an arbitrary domain value into text.
    /// </summary>
    public delegate bool Unparse(T value, out string text);

    private readonly Parse parser;
    private readonly Unparse unparser;
    private readonly T defaultValue;

    private string text;
    private T value;

    /// <summary>
    /// An optional constraint function to limit acceptable values.
    /// </summary>
    public Func<T, bool> ConstraintFn = _ => true;

    /// <summary>
    /// Construct a new ParserTextModel with a sentinel value for invalid text.
    /// </summary>
#pragma warning disable CS8601 // Possible null reference assignment.
    public ParserTextModel(Parse parser, Unparse unparser, T defaultValue = default)
#pragma warning restore CS8601 // Possible null reference assignment.
    {
        this.parser = parser;
        this.unparser = unparser;
        this.defaultValue = defaultValue;

        text = "";
        value = defaultValue;
        SetValue(defaultValue);
    }

    /// <inheritdoc/>
    public event Action<string>? OnTextValueChanged;

    /// <inheritdoc/>
    public event Action<T>? OnValueChanged;

    /// <inheritdoc/>
    public event Action<object>? OnRawValueChanged;

    /// <inheritdoc/>
    public bool IsTextValid { get; private set; } = false;

    /// <inheritdoc/>
    public string GetTextValue() => text;

    /// <inheritdoc/>
    public void SetTextValue(string value)
    {
        if (text == value)
            return;
        text = value;

        IsTextValid = parser(value, out var newValue) && ConstraintFn(newValue);
        if (!IsTextValid)
            newValue = defaultValue;

        if (!EqualityComparer<T>.Default.Equals(this.value, newValue))
        {
            this.value = newValue;
            OnValueChanged?.Invoke(this.value);
            OnRawValueChanged?.Invoke(this.value!);
        }
        OnTextValueChanged?.Invoke(text);
    }

    /// <inheritdoc/>
    public T GetValue() => Value;

    /// <inheritdoc/>
    public bool SetValue(T value)
    {
        if (EqualityComparer<T>.Default.Equals(this.value, value))
            return true;
        if (!unparser(value, out var newText) || !ConstraintFn(value))
            return false;

        IsTextValid = true;
        this.value = value;
        bool updateText = newText != text;
        if (updateText)
            text = newText;

        OnValueChanged?.Invoke(value);
        OnRawValueChanged?.Invoke(value!);
        if (updateText)
            OnTextValueChanged?.Invoke(text);
        return true;
    }

    /// <inheritdoc/>
    public LocalizedText DisplayString() => text;

    /// <inheritdoc/>
    public T Value
    {
        get => value;
        set
        {
            if (!SetValue(value))
                throw new ArgumentException($"{nameof(value)}: {value}");
        }
    }

    /// <inheritdoc/>
    public string TextValue
    {
        get => text;
        set => SetTextValue(value);
    }
}
