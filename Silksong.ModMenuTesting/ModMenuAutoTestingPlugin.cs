using BepInEx;
using UnityEngine;

namespace Silksong.ModMenuTesting;

[BepInAutoPlugin(id: "org.silksong_modding.modmenuautotesting", name: "ModMenuAutoTesting")]
public partial class ModMenuAutoTestingPlugin : BaseUnityPlugin
{
    public enum TestEnum { EnumOne, EnumTwo, EnumThree }

    private void Awake()
    {
        // All types supported by the bepinex config system


        Config.Bind("Unity Types", "KeyCode Option", KeyCode.A);
        Config.Bind("Unity Types", "Color Option", Color.green);
		Config.Bind("Unity Types", "Vector2 Option", Vector2.one); // not done
		Config.Bind("Unity Types", "Vector3 Option", Vector3.one); // not done
		Config.Bind("Unity Types", "Vector4 Option", Vector4.one); // not done
		Config.Bind("Unity Types", "Quaternion Option", Quaternion.identity); // not done


        Config.Bind("Value Types", "String Option", "value");
        Config.Bind("Value Types", "Enum Option", TestEnum.EnumOne);
        Config.Bind("Value Types", "Bool Option", true);
		Config.Bind("Value Types", "Byte Option", (byte)0); // not done
		Config.Bind("Value Types", "SByte Option", (sbyte)0); // not done
		Config.Bind("Value Types", "Short Option", (short)0); // not done
		Config.Bind("Value Types", "UShort Option", (ushort)0); // not done
		Config.Bind("Value Types", "Int Option", 0);
		Config.Bind("Value Types", "UInt Option", 0u); // not done
		Config.Bind("Value Types", "Long Option", 0L); // not done
		Config.Bind("Value Types", "ULong Option", 0UL); // not done
		Config.Bind("Value Types", "Float Option", 0.0f);
		Config.Bind("Value Types", "Double Option", 0.0d); // not done
		Config.Bind("Value Types", "Decimal Option", 0.0m); // not done


		Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}
