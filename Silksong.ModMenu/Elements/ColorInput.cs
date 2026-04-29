using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Selectable element that accepts <see cref="Color"/> input in 3, 6, or 8 character hex strings.
/// Includes a preview swatch beside the hex code.
/// </summary>
public class ColorInput : TextInput<Color>
{
    /// <summary>
    /// The relative size of the Swatch to the choice text when the input was first created.
    /// Used to automatically resize the Swatch when <see cref="SetFontSizes"/> is called.
    /// </summary>
    private readonly float swatchSizeMultiplier;

    /// <summary>
    /// Construct a color input with no description.
    /// </summary>
    public ColorInput(LocalizedText label)
        : this(label, "") { }

    /// <summary>
    /// Construct a color input with a description.
    /// </summary>
    public ColorInput(LocalizedText label, LocalizedText description)
        : base(label, TextModels.ForHexColors(), description)
    {
        Container.name = $"{label.Canonical} Color Input";
        InputField.contentType = InputField.ContentType.Custom;
        InputField.onValidateInput = HexValidation;
        ApplyDefaultColors = true;
        Format = InputFormat.RGBA;

        Swatch = MenuPrefabs.Get().NewColorSwatch().RectTransform;
        Swatch.SetParent(InputField.textComponent.transform, false);
        Swatch.gameObject.SetActive(true);
        SwatchFill = Swatch.Find("Fill").GetComponent<Image>();
        SwatchOutline = Swatch.Find("Outline").GetComponent<Image>();
        InvalidValueIndicator = Swatch.Find("Invalid Indicator").GetComponent<Text>();

        swatchSizeMultiplier = Swatch.rect.height / InputField.textComponent.preferredHeight;

        OnTextValueChanged += _ =>
        {
            SwatchFill.color = Value;
            State = TextModel.IsTextValid ? ElementState.DEFAULT : ElementState.INVALID;
            InvalidValueIndicator.enabled = !TextModel.IsTextValid;
        };

        Value = Color.clear;
    }

    /// <summary>
    /// Whether or not this input accepts 8-character RGBA strings.
    /// </summary>
    public InputFormat Format
    {
        get => field;
        set
        {
            field = value;
            InputField.characterLimit = (int)field;

            // force the field to re-clamp the length of the current text value
            string val = InputField.text;
            InputField.SetTextWithoutNotify("");
            InputField.text = val;
        }
    }

    /// <summary>
    /// Semantic input formats for <see cref="ColorInput"/>s.
    /// </summary>
    public enum InputFormat
    {
        /// <summary>
        /// The input accepts at most 6-character RGB hex codes.
        /// </summary>
        RGB = 6,

        /// <summary>
        /// The input accepts at most 8-character RGBA hex codes.
        /// </summary>
        RGBA = 8,
    }

    /// <summary>
    /// The unity component that controls the size and position of the preview swatch.
    /// </summary>
    public readonly RectTransform Swatch;

    /// <summary>
    /// The unity component for the filled area of the preview swatch.
    /// </summary>
    public readonly Image SwatchFill;

    /// <summary>
    /// The unity component for the outline around the preview swatch.
    /// </summary>
    public readonly Image SwatchOutline;

    /// <summary>
    /// The unity component for the symbol that appears in the preview swatch
    /// to visually indicate an invalid value.
    /// </summary>
    public readonly Text InvalidValueIndicator;

    /// <inheritdoc/>
    /// <remarks>
    /// This will also adjust the size and position of the <see cref="Swatch"/>.
    /// </remarks>
    public override void SetFontSizes(FontSizes fontSizes)
    {
        base.SetFontSizes(fontSizes);
        float size = InputField.textComponent.preferredHeight * swatchSizeMultiplier;
        Swatch.sizeDelta = Vector2.one * size;
        Swatch.anchoredPosition = Swatch.anchoredPosition with { x = -0.5f * size };
    }

    /// <summary>
    /// <see cref="InputField"/> validation for hex codes; only accepts characters a-fA-F0-7.
    /// </summary>
    static char HexValidation(string input, int index, char addedChar) =>
        $"{addedChar}".TryParseHex(out _) ? char.ToUpper(addedChar) : '0';
}
