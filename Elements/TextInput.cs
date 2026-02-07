using System;
using System.Collections.Generic;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Selectable element that accepts arbitrary text input.
/// </summary>
public class TextInput<T> : SelectableValueElement<T>
{
    private static readonly HashSet<Type> intTypes =
    [
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
    ];
    private static readonly HashSet<Type> floatTypes = [typeof(float), typeof(double)];

    /// <summary>
    /// Construct a basic text input.
    /// </summary>
    public TextInput(LocalizedText label, ITextModel<T> model, LocalizedText description)
        : base(MenuPrefabs.Get().NewTextInputContainer(out var inputField), inputField, model)
    {
        TextModel = model;
        var input = inputField.gameObject;

        LabelText = input.FindChild("Menu Option Label")!.GetComponent<Text>();
        DescriptionText = input.FindChild("Description")!.GetComponent<Text>();
        InputField = inputField;

        OnTextValueChanged += value => InputField.text = value;

        LabelText.LocalizedText = label;
        DescriptionText.LocalizedText = description;

        if (intTypes.Contains(typeof(T)))
            InputField.contentType = InputField.ContentType.IntegerNumber;
        else if (floatTypes.Contains(typeof(T)))
            InputField.contentType = InputField.ContentType.DecimalNumber;
    }

    /// <summary>
    /// Construct a basic text input with no description.
    /// </summary>
    public TextInput(LocalizedText label, ITextModel<T> model)
        : this(label, model, "") { }

    /// <summary>
    /// The value holder and model underlying this choice element.
    /// </summary>
    public readonly ITextModel<T> TextModel;

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
    public readonly InputField InputField;

    /// <summary>
    /// Event notified whenever the text value of the field changes.
    /// </summary>
    public event Action<string> OnTextValueChanged
    {
        add => TextModel.OnTextValueChanged += value;
        remove => TextModel.OnTextValueChanged -= value;
    }

    /// <inheritdoc/>
    public override void SetMainColor(Color color)
    {
        LabelText.color = color;
        DescriptionText.color = color;
        InputField.textComponent.color = color;
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes)
    {
        LabelText.fontSize = fontSizes.LabelSize();
        DescriptionText.fontSize = fontSizes.DescriptionSize();
        InputField.textComponent.fontSize = fontSizes.ChoiceSize();
    }
}
