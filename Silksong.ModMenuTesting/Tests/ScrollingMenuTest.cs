using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;
using System.Collections.Generic;

namespace Silksong.ModMenuTesting.Tests;

internal class ScrollingMenuTest : ModMenuTest
{
    // for finding the screen in UnityExplorer during testing
    static ScrollingMenuScreen? screen;

    private enum Spacing {
        Small,
        Medium,
        Large,
    }

    internal override string Name => "Scrolling Menu - Vertical";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        screen = new ScrollingMenuScreen("Scrolling Vertical Menu") {
            SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget
        };

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
        elementAdder.Model.OnValueChanged += value => {
            while (addedOptions.Count < value) {
                var elt = new ChoiceElement<bool>($"Extra {addedOptions.Count + 1}", ChoiceModels.ForBool(), "description");
                screen.Add(elt);
                addedOptions.Push(elt);
            }
            while (addedOptions.Count > value) {
                var elt = addedOptions.Pop();
                screen.Content.Remove(elt);
                elt.Dispose();
            }
        };
        screen.Add(elementAdder);
        elementAdder.Model.SetValue(10);
        
        return screen;
    }
}
