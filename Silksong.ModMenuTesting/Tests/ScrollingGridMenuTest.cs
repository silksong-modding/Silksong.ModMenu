using System;
using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting.Tests;

internal class ScrollingGridMenuTest : ModMenuTest
{
    private enum Spacing
    {
        Small,
        Medium,
        Large,
    }

    internal override string Name => "Scrolling Menu - Grid";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        GridGroup group = new(2);
        ScrollingGroup<GridGroup> scrollGroup = new(group);

        ListChoiceModel<Spacing>
            hModel = ChoiceModels.ForEnum<Spacing>(), vModel = ChoiceModels.ForEnum<Spacing>();
        TextButton
            hButton = new("H Space"), vButton = new("V Space"), addButton = new("Add"), removeButton = new("Remove"), addInvalidButton = new("Add Non-Atomic");

        group.Add(hButton);
        group.Add(vButton);
        group.Add(addButton);
        group.Add(removeButton);
        group.Add(addInvalidButton);

        hButton.OnSubmit = () => {
            hModel.MoveRight();
            hButton.ButtonText.text = $"H Space: {hModel.Value}";
            group.HorizontalSpacing = hModel.Value switch {
                Spacing.Small => SpacingConstants.HSPACE_SMALL,
                Spacing.Large => SpacingConstants.HSPACE_LARGE,
                _ => SpacingConstants.HSPACE_MEDIUM,
            };
            scrollGroup.ViewportSize = scrollGroup.ViewportSize with { x = group.HorizontalSpacing * group.Columns + 100 };
        };

        vButton.OnSubmit = () => {
            vModel.MoveRight();
            vButton.ButtonText.text = $"V Space: {vModel.Value}";
            group.VerticalSpacing = vModel.Value switch {
                Spacing.Small => SpacingConstants.VSPACE_SMALL,
                Spacing.Large => SpacingConstants.VSPACE_LARGE,
                _ => SpacingConstants.VSPACE_MEDIUM,
            };
        };

        Stack<IMenuEntity> addedElements = [];

        addButton.OnSubmit += () =>
        {
            TextButton button = new($"Extra {addedElements.Count + 1}");
            group.Add(button);
            addedElements.Push(button);
            addButton.ButtonText.text = $"Add (Count: {addedElements.Count})";
        };

        removeButton.OnSubmit += () =>
        {
            if (addedElements.Count > 0)
            {
                IMenuEntity button = addedElements.Pop();
                group.Remove(button);
                if (button is IDisposable x)
                    x.Dispose();
                addButton.ButtonText.text = $"Add (Count: {addedElements.Count})";
            }
        };

        addInvalidButton.OnSubmit += () => {
            VerticalGroup invalid = new();
            invalid.Add(new TextButton("Not allowed!"));
            group.Add(invalid);
            addedElements.Push(invalid);
        };

        hModel.Value = Spacing.Large;
        hButton.OnSubmit.Invoke();
        vModel.Value = Spacing.Small;
        vButton.OnSubmit.Invoke();
        for (int i = 0; i < 13; i++)
            addButton.OnSubmit.Invoke();

        return new BasicMenuScreen("Scrolling Grid Menu", scrollGroup) {
            SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget
        };
    }
}
