using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using MonoDetour;
using MonoDetour.HookGen;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Examples;
using Silksong.ModMenu.Internal;
using Silksong.ModMenu.Plugin;
using Silksong.ModMenu.Screens;
using UnityEngine;

namespace Silksong.ModMenu;

[MonoDetourTargets(typeof(UIManager), GenerateControlFlowVariants = true)] // We don't need these but another class does :')
[BepInDependency("com.bepis.bepinex.configurationmanager")]
[BepInAutoPlugin(id: "org.silksong-modding.modmenu")]
public partial class ModMenuPlugin : BaseUnityPlugin, IModMenuCustomMenu
{
    private static ModMenuPlugin? instance;

    private void Awake()
    {
        MonoDetourManager.InvokeHookInitializers(typeof(ModMenuPlugin).Assembly);
        instance = this;
    }

    public AbstractMenuScreen BuildCustomMenu()
    {
        SimpleMenuScreen menu = new("Mod Menu");

        SimpleMenuScreen example = DemoTextAdventure.BuildMenu();
        menu.Add(
            new TextButton("Browse Demo Menu")
            {
                OnSubmit = () => MenuScreenNavigation.Show(example),
            }
        );
        return menu;
    }

    public string ModMenuName() => "Mod Menu";

    internal static void LogWarning(string message)
    {
        if (instance != null)
            instance.Logger.LogWarning(message);
        else
            Debug.LogWarning(message);
    }

    internal static void LogError(string message)
    {
        if (instance != null)
            instance.Logger.LogError(message);
        else
            Debug.LogError(message);
    }

    // BepInEx logging has a 2000ms flush delay which is useless for debugging crashes, so we do a manual flush.
    internal static void FlushLogs()
    {
        foreach (var listener in BepInEx.Logging.Logger.Listeners)
        {
            if (listener is DiskLogListener disk)
                disk.LogWriter.Flush();
        }
    }

    private static void ModifyUICanvas(UIManager self)
    {
        MenuPrefabs.Load(self);

        var canvas = self.gameObject.FindChild("UICanvas")!;
        var optionsContainer = canvas.FindChild("OptionsMenuScreen")!;

        // Insert the button at the desired index.
        TextButton modOptions = new("Mods") // TODO: Support localization.
        {
            OnSubmit = () => MenuScreenNavigation.Show(GetModsMenu()),
        };
        modOptions.AddToContainer(optionsContainer.FindChild("Content")!);

        // Track the selectable at the correct index (BackButton is on the end of the list from a separate container).
        optionsContainer.GetComponent<MenuButtonList>().InsertButton(modOptions.MenuButton, 5);
    }

    private static AbstractMenuScreen? modsMenu;

    private static AbstractMenuScreen GetModsMenu()
    {
        if (modsMenu != null)
            return modsMenu;

        PaginatedMenuScreen menu = new("Mods");
        menu.Add(Registry.GenerateAllMenuElements());

        modsMenu = menu;
        menu.OnDispose += () =>
        {
            if (modsMenu == menu)
                modsMenu = null;
        };
        return menu;
    }

    [MonoDetourHookInitialize]
    private static void Hook() => Md.UIManager.Awake.Postfix(ModifyUICanvas);
}
