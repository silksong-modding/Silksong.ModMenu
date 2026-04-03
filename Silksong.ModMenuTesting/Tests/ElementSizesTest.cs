using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silksong.ModMenuTesting.Tests;

internal class ElementSizesTest : ModMenuTest
{
    internal override string Name => "ElementSizesTest";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        PaginatedMenuScreenBuilder builder = new(Name);

        TextButton makeSmall = new("Make Small");
        TextButton makeMedium = new("Make Medium");
        TextButton makeLarge = new("Make Large");

        builder.Add(makeSmall);
        builder.Add(makeMedium);
        builder.Add(makeLarge);

        List<MenuElement> elements = StandardElementsTest.CreateUnboundElements().ToList();

        makeSmall.OnSubmit += () => elements.ForEach(x => x.SetFontSizes(FontSizes.Small));
        makeMedium.OnSubmit += () => elements.ForEach(x => x.SetFontSizes(FontSizes.Medium));
        makeLarge.OnSubmit += () => elements.ForEach(x => x.SetFontSizes(FontSizes.Large));

        builder.AddRange(elements);

        return builder.Build();
    }
}
