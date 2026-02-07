using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A simple text label with no interactive functionality.
/// </summary>
public class TextLabel : MenuElement
{
    /// <summary>
    /// Construct a label with the given text contents.
    /// </summary>
    public TextLabel(LocalizedText text)
        : base(MenuPrefabs.Get().NewTextLabel())
    {
        Container.name = text.Text;
        Text = Container.GetComponent<Text>();
        Text.LocalizedText = text;
    }

    /// <summary>
    /// The unity text component of the given object.
    /// </summary>
    public readonly Text Text;

    /// <inheritdoc/>
    public override void SetMainColor(Color color) => Text.color = color;

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes) =>
        Text.fontSize = FontSizeConstants.LabelSize(fontSizes);
}
