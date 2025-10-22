using BepInEx;

namespace Silksong.ModMenu;

[BepInAutoPlugin(id: "org.silksong-modding.modmenu")]
public partial class ModMenuPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Put your initialization logic here
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}
