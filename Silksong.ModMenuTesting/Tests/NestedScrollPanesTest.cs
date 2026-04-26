using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Screens;
using System.Linq;
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
            outerContent = new() { VerticalSpacing = 460 },
            innerContentOne = new() { VerticalSpacing = SpacingConstants.VSPACE_SMALL },
            innerContentTwo = new() { VerticalSpacing = SpacingConstants.VSPACE_SMALL };

        GridGroup
            innerContentThree = new(4);

        ScrollingPane
            outerScroll = new(outerContent) { ViewportSize = new Vector2(1690, 876) },
            innerScrollOne = new(innerContentOne) { ViewportSize = new Vector2(1540, 400f) },
            innerScrollTwo = new(innerContentTwo) { ViewportSize = new Vector2(1540, 400f) },
            innerScrollThree = new(innerContentThree) { ViewportSize = new Vector2(1540, 400f) };

        foreach(int i in Enumerable.Range(1, 8))
        {
            innerContentOne.Add(new TextButton($"Hollow {i}"));
            innerContentTwo.Add(new TextButton($"Knight {i}"));
            innerContentThree.Add(new TextButton($"Silksong {i}"));
        }

        outerContent.AddRange([innerScrollOne, innerScrollTwo, innerScrollThree]);
        
        screen = new BasicMenuScreen("Nested Scroll Panes", outerScroll) {
            SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget
        };
        return screen;
    }
}
