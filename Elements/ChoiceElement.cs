using System.Collections.Generic;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A menu element with a label, a pop-up description, and a selectable value, shown as text.
/// The player can use left+right to navigate through the values as defined by the model.
/// </summary>
/// <typeparam name="T">The type of value represented.</typeparam>
public class ChoiceElement<T> : SelectableValueElement<T>
{
    public ChoiceElement(string label, IChoiceModel<T> model, string description = "")
        : base(
            MenuPrefabs.Get().NewTextChoiceContainer(out var menuOptionHorizontal),
            menuOptionHorizontal,
            model
        )
    {
        ChoiceModel = model;

        var option = menuOptionHorizontal.gameObject;
        var custom = option.AddComponent<CustomMenuOptionHorizontal>();
        custom.Model = model;

        LabelText = option.FindChild("Menu Option Label")!.GetComponent<Text>();
        DescriptionText = option.FindChild("Description")!.GetComponent<Text>();
        ChoiceText = option.FindChild("Menu Option Text")!.GetComponent<Text>();

        OnValueChanged += _ => custom.UpdateText();

        LabelText.text = label;
        DescriptionText.text = description;
        custom.UpdateText();
    }

    public ChoiceElement(string label, List<T> items, string description = "")
        : this(label, ChoiceModels.ForValues(items), description) { }

    /// <summary>
    /// The value holder and model underlying this choice element.
    /// </summary>
    public readonly IChoiceModel<T> ChoiceModel;

    /// <summary>
    /// The unity component for the label of this value choice.
    /// </summary>
    public readonly Text LabelText;

    /// <summary>
    /// The unity component for the description of this value choice.
    /// </summary>
    public readonly Text DescriptionText;

    /// <summary>
    /// The unity component for the selected value.
    /// </summary>
    public readonly Text ChoiceText;

    /// <inheritdoc/>
    public override void SetMainColor(Color color)
    {
        LabelText.color = color;
        DescriptionText.color = color;
        ChoiceText.color = color;
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes)
    {
        LabelText.fontSize = fontSizes.LabelSize();
        DescriptionText.fontSize = fontSizes.DescriptionSize();
        ChoiceText.fontSize = fontSizes.ChoiceSize();
    }
}
