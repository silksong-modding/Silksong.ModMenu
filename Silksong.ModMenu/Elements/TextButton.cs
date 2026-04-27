using System;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Screens;
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
    public TextButton(LocalizedText text, LocalizedText description)
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

        DescriptionText = MenuButton.gameObject.FindChild("Description")!.GetComponent<Text>();
        DescriptionText.LocalizedText = description;
    }

    /// <inheritdoc cref="TextButton.TextButton(LocalizedText, LocalizedText)"/>
    public TextButton(LocalizedText text)
        : this(text, string.Empty) { }

    /// <summary>
    /// Construct a text button that jumps to the given menu screen on click.
    ///
    /// The text button will have the same text as the linked screen's title.
    /// </summary>
    /// <param name="screen"></param>
    public TextButton(AbstractMenuScreen screen)
        : this(screen.TitleText.LocalizedText)
    {
        OnSubmit = () => MenuScreenNavigation.Show(screen);
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

    /// <summary>
    /// The text element for the description text.
    /// </summary>
    public readonly Text DescriptionText;

    /// <inheritdoc/>
    public override void SetMainColor(Color color)
    {
        ButtonText.color = color;
        DescriptionText.color = color;
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes)
    {
        ButtonText.fontSize = FontSizeConstants.LabelSize(fontSizes);
        DescriptionText.fontSize = FontSizeConstants.DescriptionSize(fontSizes);
    }
}
