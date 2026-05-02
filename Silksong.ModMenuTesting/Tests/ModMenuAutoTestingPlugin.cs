using BepInEx.Configuration;
using UnityEngine;

namespace Silksong.ModMenuTesting;

internal class ModMenuAutoTestingPlugin()
    : BaseProxyPluginTest("org.silksong_modding.modmenuautotesting")
{
    internal override string Name => "Mod Menu Auto Testing";

    public enum TestEnum
    {
        EnumOne,
        EnumTwo,
        EnumThree,
    }

    protected override void Setup(ConfigFile config)
    {
        config.Bind("Unity Types", "KeyCode Option", KeyCode.A);
        config.Bind("Unity Types", "Color Option", Color.green);
        config.Bind("Unity Types", "Vector2 Option", Vector2.one); // not done
        config.Bind("Unity Types", "Vector3 Option", Vector3.one); // not done
        config.Bind("Unity Types", "Vector4 Option", Vector4.one); // not done
        config.Bind("Unity Types", "Quaternion Option", Quaternion.identity); // not done

        config.Bind("Value Types", "String Option", "value");
        config.Bind("Value Types", "Enum Option", TestEnum.EnumOne);
        config.Bind("Value Types", "Bool Option", true);
        config.Bind("Value Types", "Byte Option", (byte)0); // not done
        config.Bind("Value Types", "SByte Option", (sbyte)0); // not done
        config.Bind("Value Types", "Short Option", (short)0); // not done
        config.Bind("Value Types", "UShort Option", (ushort)0); // not done
        config.Bind("Value Types", "Int Option", 0);
        config.Bind("Value Types", "UInt Option", 0u); // not done
        config.Bind("Value Types", "Long Option", 0L); // not done
        config.Bind("Value Types", "ULong Option", 0UL); // not done
        config.Bind("Value Types", "Float Option", 0.0f);
        config.Bind("Value Types", "Double Option", 0.0d); // not done
        config.Bind("Value Types", "Decimal Option", 0.0m); // not done
    }
}
