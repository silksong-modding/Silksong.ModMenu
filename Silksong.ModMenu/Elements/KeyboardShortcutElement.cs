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
    private readonly KeyboardShortcutMappableKey customMappableKey;

    /// <summary>
    /// Construct a KeyboardShortcutElement with a custom model.
    /// </summary>
    public KeyboardShortcutElement(LocalizedText label, IValueModel<KeyboardShortcut> model)
        : base(
            MenuPrefabs
                .Get()
                .NewKeyBindContainer(out KeyboardShortcutMappableKey customMappableKey),
            customMappableKey,
            model
        )
    {
        customMappableKey.Model = model;

        this.customMappableKey = customMappableKey;

        LabelText = Container.FindChild("Input Button Text")!.GetComponent<Text>();

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

    /// <inheritdoc/>
    public override void SetMainColor(Color color)
    {
        LabelText.color = color;
        customMappableKey.SetKeymapColor(color);
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes) =>
        LabelText.fontSize = fontSizes.LabelSize();
}
