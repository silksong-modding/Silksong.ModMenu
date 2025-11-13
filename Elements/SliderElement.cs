using System;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A menu element with a slider component, operating over a fixed range of integer values.
/// </summary>
public class SliderElement<T> : SelectableElement
{
    private readonly GameObject container;
    private readonly RectTransform rect;

    // Avoid playing the slider sound when modifying the slider through code instead of input.
    private bool skipPlaySlider = false;

    /// <summary>
    /// Constructs a new slider using the given model.
    /// </summary>
    /// <param name="label">The label text for the slider.</param>
    /// <param name="model">The model for the domain range and underlying value.</param>
    public SliderElement(string label, SliderModel<T> model)
    {
        container = MenuPrefabs.Get().NewSliderContainer();
        container.name = label;
        rect = container.GetComponent<RectTransform>();

        var sliderObj = container.FindChild("Slider")!;

        Slider = sliderObj.GetComponent<Slider>();
        Slider.minValue = model.MinimumIndex;
        Slider.maxValue = model.MaximumIndex;
        Slider.wholeNumbers = true;
        Slider.value = model.MinimumIndex;

        Model = model;

        LabelText = sliderObj.FindChild("Menu Option Label")!.GetComponent<Text>();
        ValueText = sliderObj.FindChild("Value")!.GetComponent<Text>();

        // Synchronize model and slider.
        OnValueChanged += _ =>
        {
            skipPlaySlider = true;
            Slider.value = model.Index;
            skipPlaySlider = false;

            UpdateValueText();
        };
        Slider.SetCallback(v =>
        {
            if (skipPlaySlider)
                skipPlaySlider = false;
            else
            {
                UIManager.instance.PlaySlider();
                model.Index = Mathf.RoundToInt(v);
            }
        });

        LabelText.text = label;
        UpdateValueText();
    }

    /// <inheritdoc/>
    public override GameObject Container => container;

    /// <inheritdoc/>
    public override RectTransform RectTransform => rect;

    /// <summary>
    /// The unity component for this slider element.
    /// </summary>
    public readonly Slider Slider;

    /// <inheritdoc/>
    public override Selectable SelectableComponent => Slider;

    /// <summary>
    /// The unity component for the label of this value choice.
    /// </summary>
    public readonly Text LabelText;

    /// <summary>
    /// The unity component for text displaying the selected value.
    /// </summary>
    public readonly Text ValueText;

    /// <summary>
    /// The underlying value model for this slider.
    /// </summary>
    public readonly SliderModel<T> Model;

    /// <summary>
    /// Listener for changes in the selected value.
    /// </summary>
    public event Action<T>? OnValueChanged
    {
        add => Model.OnValueChanged += value;
        remove => Model.OnValueChanged -= value;
    }

    /// <summary>
    /// The value currently selected by this menu element.
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
        ValueText.color = color;
    }

    /// <inheritdoc/>
    public override void SetFontSizes(FontSizes fontSizes)
    {
        LabelText.fontSize = fontSizes.LabelSize();
        ValueText.fontSize = fontSizes.SliderSize();
    }

    private void UpdateValueText() => ValueText.text = Model.DisplayString();
}
