using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Screens;
using UnityEngine;

namespace Silksong.ModMenuTesting.Tests;

internal class NestedScrollPanesTest : ModMenuTest
{
    // for finding the screen in UnityExplorer during testing
    static BasicMenuScreen? screen;

    internal override string Name => "Scrolling Menu - Nested Panes";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        VerticalGroup
            outerContent = new() { VerticalSpacing = 460 };

        GridGroup
            innerContentOne = new(2) { HorizontalSpacing = 480 },
            innerContentTwo = new(4) { HorizontalSpacing = 480 },
            innerContentThree = new(2) { HorizontalSpacing = 480 };

        ScrollingPane
            outerScroll = new(outerContent) { ViewportSize = new Vector2(1480, 860) },
            innerScrollOne = new(innerContentOne) { ViewportSize = new Vector2(1360, 350f) },
            innerScrollTwo = new(innerContentTwo)
            {
                ViewportSize = new Vector2(1360, 350f),
                Axes = ScrollingPane.ScrollAxes.Horizontal
            },
            innerScrollThree = new(innerContentThree) { ViewportSize = new Vector2(1360, 350f) };

        for (int i = 1; i <= 12; i++)
        {
            innerContentOne.Add(new TextButton($"Hollow {i}"));
            innerContentTwo.Add(new TextButton($"Knight {i}"));
            innerContentThree.Add(new TextButton($"Silksong {i}"));
        }

        outerContent.AddRange([innerScrollOne, innerScrollTwo, innerScrollThree]);
        
        screen = new BasicMenuScreen("Nested Scroll Panes", outerScroll)
        {
            SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget
        };
        return screen;
    }
}
