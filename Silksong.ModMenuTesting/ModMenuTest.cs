using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting;

internal abstract class ModMenuTest
{
    internal abstract string Name { get; }

    internal abstract AbstractMenuScreen BuildMenuScreen();
}
