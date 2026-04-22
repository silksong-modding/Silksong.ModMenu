using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;
using System;
using System.Linq;
using UnityEngine;

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

	internal override string Name => "Scrolling Menu";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
		VerticalGroup group = new();

		ChoiceElement<Spacing> spacing = new("Spacing", ChoiceModels.ForEnum<Spacing>());
		spacing.OnValueChanged += value =>
			group.VerticalSpacing = value switch
			{
				Spacing.Small => SpacingConstants.VSPACE_SMALL,
				Spacing.Large => SpacingConstants.VSPACE_LARGE,
				_ => SpacingConstants.VSPACE_MEDIUM,
			};
		group.Add(spacing);
		spacing.Model.SetValue(Spacing.Medium);

		var elementAdder = new SliderElement<int>("Extra Elements", SliderModels.ForInts(0, 20));
		elementAdder.Model.OnValueChanged += value => {
            for (int i = group.AllEntities().Count() - 1; i >= 2; i--) {
				if (group.AllEntities().ElementAt(i) is IDisposable element)
					element.Dispose();
				group.RemoveAt(i);
            }
			group.AddRange(
				Enumerable.Range(1, Mathf.Min(value, 20))
					.Select(x => new ChoiceElement<bool>($"Extra {x}", ChoiceModels.ForBool(), "description"))
			);
		};
        group.Add(elementAdder);
        elementAdder.Model.SetValue(10);
		
        screen = new ScrollingMenuScreen("Scrolling Menu Test", group);
        return screen;
    }
}
