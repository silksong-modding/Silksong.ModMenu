using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenuTesting.Tests;

internal class ScrollingPaneTests : ModMenuTest
{
    // for finding the screen in UnityExplorer during testing
    static PaginatedMenuScreen? screen;

    internal override string Name => "Scrolling Pane Tests";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        screen = new PaginatedMenuScreen("Scrolling Pane Tests")
        {
            SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget,
        };
        screen.AddPage(ScrollPaneSiblings());
        screen.AddPage(ScrollPaneNesting());
        screen.AddPage(ScrollScrollPanePane());
        screen.AddPage(OffCenterSelectables());
        screen.AddPage(DoubleAxesAutoFocus());
        return screen;
    }

    /// <summary>
    /// For testing how multiple scroll panes in one layout behave
    /// </summary>
    static GridGroup ScrollPaneSiblings()
    {
        VerticalGroup innerContentOne = new(),
            innerContentTwo = new();

        for (int i = 1; i <= 15; i++)
        {
            innerContentOne.Add(new TextButton($"Hollow {i}"));
            innerContentTwo.Add(new TextButton($"Knight {i}"));
        }

        ScrollingPane innerScrollOne =
                new(innerContentOne)
                {
                    ViewportSize = new Vector2(500, 875),
                    SmoothScrollTime = 0.5f,
                },
            innerScrollTwo =
                new(innerContentTwo)
                {
                    ViewportSize = new Vector2(500, 875),
                    SmoothScrollTime = 0.5f,
                };

        GridGroup outerContent = new(2) { HorizontalSpacing = 600 };
        outerContent.Add(innerScrollOne);
        outerContent.Add(innerScrollTwo);

        return outerContent;
    }

    /// <summary>
    /// For testing how multiple nested & sibling scroll panes of varying scroll axes in one layout behave
    /// </summary>
    static ScrollingPane ScrollPaneNesting()
    {
        VerticalGroup outerContent = new() { VerticalSpacing = 460 };

        GridGroup innerContentOne = new(2) { HorizontalSpacing = 480 },
            innerContentTwo = new(4) { HorizontalSpacing = 480 },
            innerContentThree = new(2) { HorizontalSpacing = 480 };

        ScrollingPane outerScroll =
                new(outerContent)
                {
                    ViewportSize = new Vector2(1480, 860),
                    SmoothScrollTime = 0.5f,
                },
            innerScrollOne =
                new(innerContentOne)
                {
                    ViewportSize = new Vector2(1300, 350f),
                    SmoothScrollTime = 0.5f,
                },
            innerScrollTwo =
                new(innerContentTwo)
                {
                    ViewportSize = new Vector2(1360, 350f),
                    Axes = ScrollingPane.ScrollAxes.Horizontal,
                    SmoothScrollTime = 0.5f,
                },
            innerScrollThree =
                new(innerContentThree)
                {
                    ViewportSize = new Vector2(1300, 350f),
                    SmoothScrollTime = 0.5f,
                };

        for (int i = 1; i <= 12; i++)
        {
            innerContentOne.Add(new TextButton($"Hollow {i}"));
            innerContentTwo.Add(new TextButton($"Knight {i}"));
            innerContentThree.Add(new TextButton($"Silksong {i}"));
        }

        outerContent.AddRange([innerScrollOne, innerScrollTwo, innerScrollThree]);

        return outerScroll;
    }

    /// <summary>
    /// For testing horizontal scrolling when sliders are involved
    /// (since their Selectable is offset from their visual center)
    /// </summary>
    static ScrollingPane OffCenterSelectables()
    {
        VerticalGroup group = new();

        for (int i = 1; i <= 2; i++)
        {
            group.AddRange([
                new TextButton($"Thin Button {i}"),
                new SliderElement<int>($"Wide Slider {i}", SliderModels.ForInts(0, 5)),
                new ChoiceElement<bool>($"Wide Choice {i}", ChoiceModels.ForBool()),
            ]);
        }

        ScrollingPane scroll = new(group)
        {
            ViewportSize = new Vector2(1300, 875),
            Axes = ScrollingPane.ScrollAxes.Horizontal,
        };

        scroll.scrollRect.content.GetComponent<Image>().color = new(0, 0, 1, 0.3f);
        scroll.scrollRect.viewport.GetComponent<Image>().color = new(1, 0, 0, 0.3f);
        scroll.scrollRect.viewport.GetComponent<RectMask2D>().enabled = false;

        return scroll;
    }

    /// <summary>
    /// For testing auto-focusing that requires scrolling in two axes.
    /// </summary>
    static ScrollingPane DoubleAxesAutoFocus()
    {
        FreeGroup group = new();

        TextButton tl = new("Top Left"),
            br = new("Bottom Right");

        group.Add(tl, new Vector2(-300, 0));
        group.Add(br, new Vector2(300, -600));

        tl.ConnectSymmetric(br, NavigationDirection.Right);
        tl.ConnectSymmetric(br, NavigationDirection.Down);

        ScrollingPane scroll = new(group)
        {
            ViewportSize = new Vector2(600, 500),
            Axes = ScrollingPane.ScrollAxes.Both,
        };

        scroll.scrollRect.content.GetComponent<Image>().color = new(0, 0, 1, 0.3f);
        scroll.scrollRect.viewport.GetComponent<Image>().color = new(1, 0, 0, 0.3f);
        scroll.scrollRect.viewport.GetComponent<RectMask2D>().enabled = false;

        return scroll;
    }

    /// <summary>
    /// For testing how scroll panes whose content is another scroll pane behave
    /// </summary>
    static ScrollingPane ScrollScrollPanePane()
    {
        VerticalGroup content = new();
        for (int i = 1; i <= 15; i++)
            content.Add(new TextButton($"Test {i}"));

        ScrollingPane scrollOne =
                new(content) { ViewportSize = new Vector2(1100, 1075), SmoothScrollTime = 0.5f },
            scrollTwo =
                new(scrollOne) { ViewportSize = new Vector2(1300, 975), SmoothScrollTime = 0.5f },
            finalScroll =
                new(scrollTwo) { ViewportSize = new Vector2(1500, 875), SmoothScrollTime = 0.5f };

        return finalScroll;
    }
}
