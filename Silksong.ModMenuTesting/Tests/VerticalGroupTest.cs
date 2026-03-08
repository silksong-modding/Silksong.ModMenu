using System;
using System.Linq;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting.Tests;

internal enum VerticalGroupSpacing
{
    Small,
    Medium,
    Large,
}

internal class VerticalGroupTest : ModMenuTest
{
    internal override string Name => "Vertical Group";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        VerticalGroup group = new();

        TextButton resetButton = new("Reset");
        void Reset()
        {
            foreach (var element in group.AllElements())
                if (element != resetButton)
                    element.Dispose();
            group.Clear();

            ChoiceElement<VerticalGroupSpacing> spacing = new(
                "Spacing",
                ChoiceModels.ForEnum<VerticalGroupSpacing>()
            );
            spacing.OnValueChanged += value =>
                group.VerticalSpacing = value switch
                {
                    VerticalGroupSpacing.Small => SpacingConstants.VSPACE_SMALL,
                    VerticalGroupSpacing.Medium => SpacingConstants.VSPACE_MEDIUM,
                    VerticalGroupSpacing.Large => SpacingConstants.VSPACE_LARGE,
                    _ => throw new ArgumentException($"{value}"),
                };
            spacing.Value = VerticalGroupSpacing.Medium;
            group.VerticalSpacing = SpacingConstants.VSPACE_MEDIUM;
            group.Add(spacing);

            TextButton insertBelow = new("Insert Below");
            insertBelow.OnSubmit += () => group.Insert(2, new TextButton("New Button"));
            group.Add(insertBelow);

            TextButton removeThird = new("Remove Third Button");
            removeThird.OnSubmit += () => group.RemoveAt(2);
            group.Add(removeThird);

            TextButton maybeVisible = new("Maybe Visible");
            group.Add(maybeVisible);

            ChoiceElement<bool> isAboveVisible = new("Is Above Visible?", ChoiceModels.ForBool());
            isAboveVisible.OnValueChanged += value => maybeVisible.VisibleSelf = value;
            isAboveVisible.Value = true;
            group.Add(isAboveVisible);

            ChoiceElement<bool> hideGaps = new("Hide Gaps", ChoiceModels.ForBool());
            hideGaps.OnValueChanged += value => group.HideInactiveElements = value;
            group.HideInactiveElements = false;
            hideGaps.Value = false;
            group.Add(hideGaps);

            group.Add(resetButton);
        }
        resetButton.OnSubmit += Reset;
        Reset();

        return new BasicMenuScreen("Vertical Group Test", group);
    }
}
