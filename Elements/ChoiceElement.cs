using System;
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
public class ChoiceElement<T> : SelectableElement
{
    private readonly GameObject container;
    private readonly RectTransform rect;
    private readonly MenuOptionHorizontal menuOptionHorizontal;

    public ChoiceElement(string label, IChoiceModel<T> model, string description = "")
    {
        Model = model;
        container = MenuPrefabs.Get().NewTextChoiceContainer();
        container.name = label;
        rect = container.GetComponent<RectTransform>();

        var option = container.FindChild("ValueChoice")!;

        menuOptionHorizontal = option.GetComponent<MenuOptionHorizontal>();
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

    /// <inheritdoc/>
    public override GameObject Container => container;

    /// <inheritdoc/>
    public override RectTransform RectTransform => rect;

    /// <inheritdoc/>
    public override Selectable SelectableComponent => menuOptionHorizontal;

    /// <summary>
    /// The value holder and model underlying this choice element.
    /// </summary>
    public readonly IChoiceModel<T> Model;

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

    /// <summary>
    /// Listener for changes in the selected value.
    /// </summary>
    public event Action<T>? OnValueChanged
    {
        add => Model.OnValueChanged += value;
        remove => Model.OnValueChanged -= value;
    }

    /// <summary>
    /// The value chosen by this menu element.
    /// </summary>
    public T Value
    {
        get => Model.Value;
        set => Model.Value = value;
    }

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
