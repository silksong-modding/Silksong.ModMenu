using TeamCherry.Localization;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Wrapper class for either text that supports localization, or raw text which does not.
/// </summary>
public class LocalizedText
{
    private readonly TeamCherry.Localization.LocalisedString localisedString;
    private readonly string rawText;

    private LocalizedText(LocalisedString localisedString, string rawText)
    {
        this.localisedString = localisedString;
        this.rawText = rawText;
    }

    /// <summary>
    /// Get the readable text represented by this object.
    /// </summary>
    public string Text =>
        localisedString.IsEmpty
            ? rawText
            : Language.Get(localisedString.Key, localisedString.Sheet);

    /// <summary>
    /// Returns true if this object has localization support.
    /// </summary>
    public bool IsLocalized => !localisedString.IsEmpty;

    /// <summary>
    /// Returns the localized identifier for this object, which may be empty.
    /// </summary>
    public LocalisedString Localized => localisedString;

    /// <summary>
    /// Represents localized text with the given key and an empty sheet title.
    /// </summary>
    public static LocalizedText Key(string languageKey) => new(new("", languageKey), "");

    /// <summary>
    /// Represents localized text with the given key.
    /// </summary>
    public static LocalizedText Key(LocalisedString localisedString) => new(localisedString, "");

    /// <summary>
    /// Represents text with no localization that always renders to the given value.
    /// </summary>
    public static LocalizedText Raw(string rawText) => new(new(), rawText);

    /// <summary>
    /// Implicit conversion for raw text to un-localized LocalizedText.
    /// </summary>
    public static implicit operator LocalizedText(string rawText) => Raw(rawText);

    /// <summary>
    /// Implicit conversion from a LocalisedString.
    /// </summary>
    public static implicit operator LocalizedText(LocalisedString localisedString) =>
        Key(localisedString);
}
