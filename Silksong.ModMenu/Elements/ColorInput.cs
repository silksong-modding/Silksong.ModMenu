using System.Globalization;
using System.Linq;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
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
    /// Construct a color input with no description.
    /// </summary>
    public ColorInput(LocalizedText label)
        : this(label, (LocalizedText)"") { }

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

        Swatch = MenuPrefabs.Get().NewColorSwatch();
        Swatch.transform.SetParent(InputField.textComponent.transform, false);
        Swatch.SetActive(true);
        SwatchFill = Swatch.FindChild("Fill")!.GetComponent<Image>();
        SwatchOutline = Swatch.FindChild("Outline")!.GetComponent<Image>();
        InvalidValueIndicator = Swatch.FindChild("Invalid Indicator")!.GetComponent<Text>();

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
    /// The GameObject that controls the size and position of the preview swatch.
    /// </summary>
    public readonly GameObject Swatch;

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

    /// <summary>
    /// <see cref="InputField"/> validation for hex codes; only accepts characters a-fA-F0-7.
    /// </summary>
    static char HexValidation(string input, int index, char addedChar) =>
        $"{addedChar}".TryParseHex(out _) ? char.ToUpper(addedChar) : '0';
}
