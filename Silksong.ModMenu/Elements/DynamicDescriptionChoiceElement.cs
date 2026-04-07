using System;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Subclass of <see cref="ChoiceElement{T}"/> with a second description object that lies below the choice text.
/// </summary>
public class DynamicDescriptionChoiceElement<T> : ChoiceElement<T>
{
    internal const string RIGHT_DESCRIPTION_NAME = "ModMenu-Right Description";

    /// <inheritdoc cref="ChoiceElement{T}.ChoiceElement(LocalizedText, IChoiceModel{T}, LocalizedText)"/>
    public DynamicDescriptionChoiceElement(
        LocalizedText label,
        IChoiceModel<T> model,
        LocalizedText description,
        LocalizedText rightDescription
    )
        : base(label, model, description)
    {
        RightText = SetupRightDescription(DescriptionText, ChoiceText, MenuOptionComponent);
        RightText.LocalizedText = rightDescription;
    }

    /// <summary>
    /// Create a choice element with a description below the choice that updates based on the value of the underlying Model.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="model"></param>
    /// <param name="description"></param>
    /// <param name="getRightDescription">Function used to determine the description below the choice.</param>
    public DynamicDescriptionChoiceElement(
        LocalizedText label,
        IChoiceModel<T> model,
        LocalizedText description,
        Func<T, LocalizedText> getRightDescription
    )
        : this(label, model, description, getRightDescription(model.Value))
    {
        model.OnValueChanged += value => RightText.LocalizedText = getRightDescription(value);
    }

    /// <summary>
    /// The Unity component for the text object added on the right hand side (below the <see cref="ChoiceElement{T}.ChoiceText"/>).
    /// </summary>
    public readonly Text RightText;

    private static Text SetupRightDescription(
        Text descriptionText,
        Text choiceText,
        MenuSelectable selectable
    )
    {
        GameObject desc = descriptionText.gameObject;
        GameObject rightDesc = UObject.Instantiate(
            desc,
            desc.transform.parent,
            worldPositionStays: true
        );
        rightDesc.name = RIGHT_DESCRIPTION_NAME;
        rightDesc.SetActive(true);
        Text rightText = rightDesc.GetComponent<Text>();
        rightText.alignment = TextAnchor.MiddleRight;

        RectTransform originalRect = desc.GetComponent<RectTransform>();
        RectTransform newRect = rightDesc.GetComponent<RectTransform>();
        RectTransform optionRect = choiceText.gameObject.GetComponent<RectTransform>();

        newRect.anchorMin = new(1f, 0.5f);
        newRect.anchorMax = new(1f, 0.5f);
        newRect.pivot = new(1f, 0.5f);
        newRect.anchoredPosition = new(
            optionRect.anchoredPosition.x + ((1f - optionRect.pivot.x) * optionRect.sizeDelta.x),
            originalRect.anchoredPosition.y
        );

        if (rightDesc.TryGetComponent<ChangePositionByLanguage>(out var cpbl))
        {
            cpbl.originalPosition = rightDesc.transform.localPosition;
        }

        selectable
            .gameObject.GetOrAddComponent<MenuSelectableAnimationProxy>()
            .Animators.Add(rightDesc.GetComponent<Animator>());

        return rightText;
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes)
    {
        base.SetFontSizes(fontSizes);
        RightText.fontSize = fontSizes.DescriptionSize();
    }

    /// <inheritdoc/>
    public override void SetMainColor(Color color)
    {
        base.SetMainColor(color);
        RightText.color = color;
    }
}
