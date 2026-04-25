using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;
using System.Linq;
using UnityEngine;

namespace Silksong.ModMenuTesting.Tests;

internal class MultipleScrollPanesTest : ModMenuTest
{
    // for finding the screen in UnityExplorer during testing
    static BasicMenuScreen? screen;

    internal override string Name => "Multiple Scroll Panes";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        VerticalGroup
            outerContent = new() { VerticalSpacing = 460 },
            innerContentOne = new() { VerticalSpacing = SpacingConstants.VSPACE_SMALL },
            innerContentTwo = new() { VerticalSpacing = SpacingConstants.VSPACE_SMALL };

        ScrollingPane
            innerScrollOne = new(innerContentOne) { ViewportSize = new Vector2(1540, 400f) },
            innerScrollTwo = new(innerContentTwo) { ViewportSize = new Vector2(1540, 400f) };

        AddSomeElements(innerContentOne, 10, "Foo");
        AddSomeElements(innerContentTwo, 10, "Bar");

        outerContent.AddRange([innerScrollOne, innerScrollTwo]);
        
        screen = new BasicMenuScreen("Multiple Scroll Panes", outerContent) {
            SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget
        };
        return screen;
    }

    static void AddSomeElements(VerticalGroup group, int count, string name) =>
        group.AddRange(
            Enumerable.Range(1, count).Select(i => new ChoiceElement<bool>($"{name} {i}", ChoiceModels.ForBool()))
        );
}
