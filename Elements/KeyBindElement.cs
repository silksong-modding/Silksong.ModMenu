using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Element for selecting a key code via input capture.
/// </summary>
public class KeyBindElement : SelectableValueElement<KeyCode>
{
    /// <summary>
    /// Construct a KeyBindElement with a custom model.
    /// </summary>
    public KeyBindElement(string label, IValueModel<KeyCode> model)
        : base(
            MenuPrefabs.Get().NewKeyBindContainer(out var customMappableKey),
            customMappableKey,
            model
        )
    {
        customMappableKey.KeyCodeModel = model;

        LabelText = Container.FindChild("Input Button Text")!.GetComponent<Text>();
        KeyBindText = customMappableKey.KeymapText!;
        KeyBindImage = customMappableKey.KeymapImage!;

        LabelText.text = label;
    }

    /// <summary>
    /// Construct a KeyBindElement with a default model that accepts any KeyCode.
    /// </summary>
    public KeyBindElement(string label)
        : this(label, new ValueModel<KeyCode>(KeyCode.A)) { }

    /// <summary>
    /// The unity component for the label of this value choice.
    /// </summary>
    public readonly Text LabelText;

    /// <summary>
    /// The unity component for the text of the selected key bind.
    /// </summary>
    public readonly Text KeyBindText;

    /// <summary>
    /// The unity component for the image of the selected key bind.
    /// </summary>
    public readonly Image KeyBindImage;

    /// <inheritdoc/>
    public override void SetMainColor(Color color)
    {
        LabelText.color = color;
        KeyBindText.color = color;
        KeyBindImage.color = color;
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes) =>
        LabelText.fontSize = fontSizes.LabelSize();
}
