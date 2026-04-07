using System;
using System.Collections.Generic;
using System.Text;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting.Tests;

internal class StandardElementsTest : ModMenuTest
{
    private static void Log(string message) => ModMenuTestingPlugin.InstanceLogger.LogInfo(message);

    internal override string Name => "Standard Elements";

    /// <summary>
    /// Yields common elements that log their value when changed.
    /// </summary>
    internal static IEnumerable<MenuElement> CreateUnboundElements()
    {
        {
            TextButton button = new("The Text Button", "Here is a text button");
            button.OnSubmit += () => Log($"Pressed text button");
            yield return button;
        }

        {
            IntSliderModel sliderModel = new(3, 8);
            SliderElement<int> intSliderElement = new("The Int Slider", sliderModel);
            sliderModel.SetValue(6);
            sliderModel.OnValueChanged += n => Log($"Int slider -> {n}");
            yield return intSliderElement;
        }

        {
            ListChoiceModel<string> listChoiceModel = new(["First", "Second", "Third"]);
            ChoiceElement<string> choiceElement = new(
                "The List Choice",
                listChoiceModel,
                "Here is where to choose option(s)"
            );
            listChoiceModel.OnValueChanged += v => Log($"List choice -> {v}");
            yield return choiceElement;
        }

        {
            TextLabel label = new("The Label");
            // Commented out because this is bugged ATM
            // yield return label;
        }

        {
            KeyBindElement keybindElement = new("The Keybind");
            keybindElement.Model.OnValueChanged += k => Log($"Keybind value -> {k}");
            yield return keybindElement;
        }

        {
            ITextModel<string> textModel = TextModels.ForStrings();
            TextInput<string> stringInput = new(
                "The Text Input",
                textModel,
                "Here is where to input text"
            );
            textModel.OnValueChanged += s => Log($"Text model -> {s}");
            yield return stringInput;
        }
    }

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        PaginatedMenuScreenBuilder builder = new(Name);
        builder.AddRange(CreateUnboundElements());
        return builder.Build();
    }
}
