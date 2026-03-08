using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Plugin;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenuTesting;

[BepInAutoPlugin(id: "org.silksong_modding.modmenutesting")]
public partial class ModMenuTestingPlugin : BaseUnityPlugin, IModMenuCustomMenu
{
    private void Awake()
    {
        // Put your initialization logic here
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private static IEnumerable<ModMenuTest> CreateTests()
    {
        foreach (
            var type in Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ModMenuTest)))
        )
            yield return (ModMenuTest)Activator.CreateInstance(type);
    }

    public AbstractMenuScreen BuildCustomMenu()
    {
        PaginatedMenuScreenBuilder builder = new("Mod Menu Testing");
        foreach (var test in CreateTests().OrderBy(t => t.Name))
        {
            TextButton button = new(test.Name);

            List<AbstractMenuScreen> screen = [];
            var testCopy = test;
            button.OnSubmit += () =>
            {
                if (screen.Count == 0)
                {
                    try
                    {
                        screen.Add(testCopy.BuildMenuScreen());
                    }
                    catch (Exception ex)
                    {
                        button.State = ElementState.INVALID;
                        UnityEngine.Debug.LogError(
                            $"Failed to build mod menu for '{testCopy.Name}': {ex}"
                        );
                    }
                }

                if (screen.Count > 0)
                    MenuScreenNavigation.Show(screen[0]);
            };

            builder.Add(button);
        }

        return builder.Build();
    }

    public string ModMenuName() => "Mod Menu Testing";
}
