using System;
using Silksong.ModMenu.Internal;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A simple text element which can be selected and pressed.
/// </summary>
public class TextButton : SelectableElement
{
    /// <summary>
    /// Construct a text button with the given text.
    /// </summary>
    public TextButton(LocalizedText text)
        : base(MenuPrefabs.Get().NewTextButtonContainer(out var menuButton), menuButton)
    {
        Container.name = text.Text;

        MenuButton = menuButton;
        MenuButton
            .GetComponent<EventTrigger>()
            .SetCallback(() =>
            {
                if (Interactable)
                    OnSubmit?.Invoke();
            });

        // The only benefit of marking a button as 'Proceed' is it makes the selection markers go away early when switching menus.
        // The downside of marking a button 'Proceed' incorrectly is that it softlocks Controller users when pressed.
        // Given this trade off, it doesn't seem worth making this a configurable option.
        MenuButton.buttonType = MenuButton.MenuButtonType.Activate;

        ButtonText = menuButton.gameObject.FindChild("Menu Button Text")!.GetComponent<Text>();
        ButtonText.LocalizedText = text;
    }

    /// <summary>
    /// The action(s) to perform when this button is selected.
    /// This takes place on the UI Thread and so must be relatively instantaneous.
    /// </summary>
    public Action? OnSubmit;

    /// <summary>
    /// The unity component for this button.
    /// </summary>
    public readonly MenuButton MenuButton;

    /// <summary>
    /// The actual text element of this button.
    /// </summary>
    public readonly Text ButtonText;

    /// <inheritdoc/>
    public override void SetMainColor(Color color) => ButtonText.color = color;

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes) =>
        ButtonText.fontSize = FontSizeConstants.LabelSize(fontSizes);
}
