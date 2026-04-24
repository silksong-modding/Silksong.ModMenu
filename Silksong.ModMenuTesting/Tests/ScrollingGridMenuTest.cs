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
        ScrollingGridGroup group = new(2);
		var screen = new BasicMenuScreen("Scrolling Grid Menu", group) {
			SelectOnShowBehaviour = SelectOnShowBehaviour.NeverForget
		};

		ListChoiceModel<Spacing>
            hModel = ChoiceModels.ForEnum<Spacing>(), vModel = ChoiceModels.ForEnum<Spacing>();
        TextButton
            hButton = new("H Space"), vButton = new("V Space"), addButton = new("Add"), removeButton = new("Remove");

		group.Add(hButton);
        group.Add(vButton);
        group.Add(addButton);
        group.Add(removeButton);

        hButton.OnSubmit = () => {
            hModel.MoveRight();
            hButton.ButtonText.text = $"H Space: {hModel.Value}";
            group.HorizontalSpacing = hModel.Value switch {
                Spacing.Small => SpacingConstants.HSPACE_SMALL,
                Spacing.Large => SpacingConstants.HSPACE_LARGE,
                _ => SpacingConstants.HSPACE_MEDIUM,
            };
            group.ViewportSize = group.ViewportSize with { x = group.HorizontalSpacing * group.Columns + 100 };
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

        Stack<TextButton> addedButtons = [];

        addButton.OnSubmit += () =>
        {
            TextButton button = new($"Extra {addedButtons.Count + 1}");
            group.Add(button);
            addedButtons.Push(button);
            addButton.ButtonText.text = $"Add (Count: {addedButtons.Count})";
        };

        removeButton.OnSubmit += () =>
        {
            if (addedButtons.Count > 0)
            {
                TextButton button = addedButtons.Pop();
                group.Remove(button);
                button.Dispose();
				addButton.ButtonText.text = $"Add (Count: {addedButtons.Count})";
			}
        };

        hModel.Value = Spacing.Large;
        hButton.OnSubmit.Invoke();
        vModel.Value = Spacing.Small;
		vButton.OnSubmit.Invoke();
        for (int i = 0; i < 13; i++)
            addButton.OnSubmit.Invoke();

		return screen;
    }
}
