using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting.Tests;

internal class ScrollingMenuTest : ModMenuTest
{
    // for finding the screen in UnityExplorer during testing
    static ScrollingMenuScreen? screen;

    private enum Spacing
    {
        Small,
        Medium,
        Large,
    }

    internal override string Name => "Scrolling Menu Test";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        screen = new ScrollingMenuScreen("Scrolling Menu Test")
        {
            SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget,
        };

        // Despite this setting, we should not see the horizontal scrollbar,
        // because no element should be wider than the default width of a scroll pane.
        screen.ScrollPane.Axes = ScrollingPane.ScrollAxes.Both;

        ChoiceElement<Spacing> spacing = new("Spacing", ChoiceModels.ForEnum<Spacing>());
        spacing.OnValueChanged += value =>
            screen.Content.VerticalSpacing = value switch
            {
                Spacing.Small => SpacingConstants.VSPACE_SMALL,
                Spacing.Large => SpacingConstants.VSPACE_LARGE,
                _ => SpacingConstants.VSPACE_MEDIUM,
            };
        screen.Add(spacing);
        spacing.Model.SetValue(Spacing.Medium);

        Stack<ChoiceElement<bool>> addedOptions = [];

        var elementAdder = new SliderElement<int>("Extra Elements", SliderModels.ForInts(0, 20));
        elementAdder.Model.OnValueChanged += value =>
        {
            while (addedOptions.Count < value)
            {
                var elt = new ChoiceElement<bool>(
                    $"Extra {addedOptions.Count + 1}",
                    ChoiceModels.ForBool(),
                    "description"
                );
                screen.Add(elt);
                addedOptions.Push(elt);
            }
            while (addedOptions.Count > value)
            {
                var elt = addedOptions.Pop();
                screen.Content.Remove(elt);
                elt.Dispose();
            }
        };
        screen.Add(elementAdder);

        // Adding one of everything to make sure the horizontal scrollbar doesn't appear
        screen.Add(new TextLabel("Label"));
        screen.Add(new TextButton("Button"));
        screen.Add(new KeyBindElement("KeyBind"));
        screen.Add(
            new DynamicDescriptionChoiceElement<bool>(
                "DynamicChoice",
                ChoiceModels.ForBool(),
                "desc left",
                "desc right"
            )
        );

        var textinput = new TextInput<string>("Input", TextModels.ForStrings());
        textinput.Model.Value = "placeholder";
        screen.Add(textinput);

        elementAdder.Model.SetValue(5);

        return screen;
    }
}
