using BepInEx.Configuration;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Element for selecting a KeyboardShortcut via input capture.
/// </summary>
public class KeyboardShortcutElement : SelectableValueElement<KeyboardShortcut>
{
    /// <summary>
    /// Construct a KeyboardShortcutElement with a custom model.
    /// </summary>
    public KeyboardShortcutElement(LocalizedText label, IValueModel<KeyboardShortcut> model)
        : base(
            MenuPrefabs.Get().NewKeyBindContainer(out var customMappableKey),
            customMappableKey,
            model
        )
    {
        customMappableKey.KeyCodeModel = new KeyboardShortcutModel(model);

        LabelText = Container.FindChild("Input Button Text")!.GetComponent<Text>();
        KeyboardShortcutText = customMappableKey.KeymapText!;
        KeyboardShortcutImage = customMappableKey.KeymapImage!;

        LabelText.LocalizedText = label;
    }

    /// <summary>
    /// Construct a KeyboardShortcutElement with a default model that accepts any KeyboardShortcut.
    /// </summary>
    public KeyboardShortcutElement(LocalizedText label)
        : this(label, new ValueModel<KeyboardShortcut>(KeyboardShortcut.Empty)) { }

    /// <summary>
    /// The unity component for the label of this value choice.
    /// </summary>
    public readonly Text LabelText;

    /// <summary>
    /// The unity component for the text of the selected key bind.
    /// </summary>
    public readonly Text KeyboardShortcutText;

    /// <summary>
    /// The unity component for the image of the selected key bind.
    /// </summary>
    public readonly Image KeyboardShortcutImage;

    /// <inheritdoc/>
    public override void SetMainColor(Color color)
    {
        LabelText.color = color;
        KeyboardShortcutText.color = color;
        KeyboardShortcutImage.color = color;
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes) =>
        LabelText.fontSize = fontSizes.LabelSize();
}

internal class KeyboardShortcutModel(IValueModel<KeyboardShortcut> model)
    : AbstractValueModel<KeyCode>
{
    public override KeyCode GetValue() => model.Value.MainKey;

    public override bool SetValue(KeyCode value) => model.SetValue(new KeyboardShortcut(value));
}
