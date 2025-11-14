using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// A menu element with a slider component, operating over a fixed range of integer values.
/// </summary>
public class SliderElement<T> : SelectableValueElement<T>
{
    // Avoid playing the slider sound when modifying the slider through code instead of input.
    private bool skipPlaySlider = false;

    /// <summary>
    /// Constructs a new slider using the given model.
    /// </summary>
    /// <param name="label">The label text for the slider.</param>
    /// <param name="model">The model for the domain range and underlying value.</param>
    public SliderElement(string label, SliderModel<T> model)
        : base(MenuPrefabs.Get().NewSliderContainer(out var slider), slider, model)
    {
        Container.name = label;

        Slider = slider;
        var sliderObj = Slider.gameObject;

        Slider.minValue = model.MinimumIndex;
        Slider.maxValue = model.MaximumIndex;
        Slider.wholeNumbers = true;
        Slider.value = model.MinimumIndex;

        SliderModel = model;

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

    /// <summary>
    /// The unity component for this slider element.
    /// </summary>
    public readonly Slider Slider;

    /// <summary>
    /// The underlying value model for this slider.
    /// </summary>
    public readonly SliderModel<T> SliderModel;

    /// <summary>
    /// The unity component for the label of this value choice.
    /// </summary>
    public readonly Text LabelText;

    /// <summary>
    /// The unity component for text displaying the selected value.
    /// </summary>
    public readonly Text ValueText;

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

    private void UpdateValueText() => ValueText.text = SliderModel.DisplayString();
}
