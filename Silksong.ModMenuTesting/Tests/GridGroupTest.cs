using System;
using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting.Tests;

internal class GridGroupTest : ModMenuTest
{
    private enum Spacing
    {
        Small,
        Medium,
        Large,
    }

    internal override string Name => "Grid Group";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        GridGroup group = new(2);

        group.Add(new TextButton("Button 1"));
        group.Add(new TextButton("Button 2"));

        ChoiceElement<Spacing> hSpacing = new(
            "Horizontal Spacing",
            ChoiceModels.ForEnum<Spacing>()
        );
        hSpacing.OnValueChanged += value =>
            group.HorizontalSpacing = value switch
            {
                Spacing.Small => SpacingConstants.HSPACE_SMALL,
                Spacing.Medium => SpacingConstants.HSPACE_MEDIUM,
                Spacing.Large => SpacingConstants.HSPACE_LARGE,
                _ => throw new ArgumentException($"{value}"),
            };
        hSpacing.Value = Spacing.Medium;
        group.HorizontalSpacing = SpacingConstants.HSPACE_MEDIUM;
        group.Add(hSpacing);
        group.Add(new TextButton(""));

        ChoiceElement<Spacing> vSpacing = new("Vertical Spacing", ChoiceModels.ForEnum<Spacing>());
        vSpacing.OnValueChanged += value =>
            group.VerticalSpacing = value switch
            {
                Spacing.Small => SpacingConstants.VSPACE_SMALL,
                Spacing.Medium => SpacingConstants.VSPACE_MEDIUM,
                Spacing.Large => SpacingConstants.VSPACE_LARGE,
                _ => throw new ArgumentException($"{value}"),
            };
        vSpacing.Value = Spacing.Medium;
        group.VerticalSpacing = SpacingConstants.VSPACE_MEDIUM;
        group.Add(vSpacing);
        group.Add(new TextButton(""));

        Stack<TextButton> addedButtons = [];

        TextButton addButton = new("Add");
        addButton.OnSubmit += () =>
        {
            TextButton button = new("New Button");
            group.Add(button);
            addedButtons.Push(button);
        };
        group.Add(addButton);

        TextButton removeButton = new("Remove");
        removeButton.OnSubmit += () =>
        {
            if (addedButtons.Count > 0)
            {
                TextButton button = addedButtons.Pop();
                group.Remove(button);
                button.Dispose();
            }
        };
        group.Add(removeButton);

        return new BasicMenuScreen("Grid Group Test", group);
    }
}
