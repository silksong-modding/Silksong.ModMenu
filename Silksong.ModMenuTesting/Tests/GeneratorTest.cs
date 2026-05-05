using System.ComponentModel;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Generator;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting.Tests;

internal class GeneratorTest : ModMenuTest
{
    internal override string Name => "Generator Test";

    private readonly GeneratedData data = new();

    internal override AbstractMenuScreen BuildMenuScreen()
    {
        PaginatedMenuScreenBuilder builder = new("Generator Test");

        GeneratedDataMenu menu = new();
        menu.ApplyFrom(data);
        menu.OnValueChanged += e =>
        {
            menu.ExportTo(data);
            data.NumTimesChanged++;
            menu.ApplyFrom(data);
        };

        builder.AddRange(menu.Elements());
        return builder.Build();
    }
}

[GenerateMenu]
public class GeneratedData
{
    [ModMenuIgnore]
    public int IgnoredIntValue;

    public int NumTimesChanged = 0;

    public sbyte SignedByteValue = 1;
    public byte ByteValue = 2;
    public short ShortValue = 3;
    public ushort UnsignedShortValue = 4;
    public int IntValue = 5;
    public uint UnsignedIntValue = 6;
    public long LongValue = 7;
    public ulong UnsignedLongValue = 8;
    public float FloatValue = 9;
    public double DoubleValue = 10;

    [ModMenuName("Custom Bool Name")]
    public bool BoolValue;

    [ElementFactory<SliderFactory>]
    public int CustomIntValue = 5;

    public string TextValue = "foo";

    [Description("Hi I'm a property!")]
    public int IntProperty
    {
        get => field + 1;
        set => field = value - 1;
    }

    [ModMenuOptions(2, 3, 5, 7)]
    public int PrimeInt;

    [SubMenu<SubMenuDataMenu>]
    public SubMenuData SubMenu1 = new();

    [SubMenu<SubMenuDataMenu>]
    public SubMenuData SubMenu2 = new();
}

public class SliderFactory : IElementFactory<int, SliderElement<int>>
{
    public SliderElement<int> CreateElement(LocalizedText name, LocalizedText description) =>
        new(name, SliderModels.ForInts(0, 100));
}

[GenerateMenu]
public class SubMenuData
{
    public string A = "A";
    public string B = "B";

    [ModMenuRange(2, 7)]
    public int Ranged = 2;
}
