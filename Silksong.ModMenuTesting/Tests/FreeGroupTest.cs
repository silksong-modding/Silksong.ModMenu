using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting.Tests;

internal class FreeGroupTest : ModMenuTest
{
    internal override string Name => "Free Group";

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        FreeGroup group = new();

        TextButton topLeft = new("Top Left");
        group.Add(topLeft, new(-800, 0));

        List<bool> toggled = [true];
        TextButton toggleTopLeft = new("Toggle Top Left");
        toggleTopLeft.OnSubmit += () =>
        {
            toggled[0] = !toggled[0];
            if (toggled[0])
                group.Add(topLeft, new(-800, 0));
            else
                group.Remove(topLeft);
        };
        group.Add(toggleTopLeft, new(-800, -200));

        TextButton middle = new("Middle");
        group.Add(middle, new(0, -400));

        TextButton moveLeft = new("Move Middle Left");
        moveLeft.OnSubmit += () =>
        {
            group.TryGetOffset(middle, out var offset);
            offset.x -= 25;
            group.Update(middle, offset);
        };
        group.Add(moveLeft, new(-800, -600));

        TextButton moveRight = new("Move Middle Right");
        moveRight.OnSubmit += () =>
        {
            group.TryGetOffset(middle, out var offset);
            offset.x += 25;
            group.Update(middle, offset);
        };
        group.Add(moveRight, new(800, -600));

        return new BasicMenuScreen("Free Group Test", group);
    }
}
