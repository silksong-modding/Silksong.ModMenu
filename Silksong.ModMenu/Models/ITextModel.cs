using System;

namespace Silksong.ModMenu.Models;

/// <summary>
/// Model for a text input field.
/// </summary>
public interface ITextModel<T> : IValueModel<T>
{
    /// <summary>
    /// Returns true if the current text value successfully parses to a domain value.
    /// </summary>
    bool IsTextValid { get; }

    /// <summary>
    /// Event notified whenever the text value of this model changes.
    /// </summary>
    event Action<string>? OnTextValueChanged;

    /// <summary>
    /// Gets the current text value of the field.
    /// </summary>
    string GetTextValue();

    /// <summary>
    /// Set the current text value of the field, updating the domain value if possible.
    /// </summary>
    void SetTextValue(string value);

    /// <summary>
    /// Convenience accessor for the text value of the model.
    /// </summary>
    string TextValue { get; set; }
}
