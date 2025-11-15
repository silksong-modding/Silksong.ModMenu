using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silksong.UnityHelper.Extensions;

namespace Silksong.ModMenu.Internal;

internal static class UIManagerUtil
{
    private static readonly FieldInfo isFadingMenuField = typeof(UIManager).GetField(
        "isFadingMenu",
        BindingFlags.NonPublic | BindingFlags.Instance
    );
    private static readonly List<FieldInfo> menuScreenFields =
    [
        .. typeof(UIManager)
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(f => f.FieldType == typeof(MenuScreen)),
    ];

    internal static void SetIsFadingMenu(this UIManager self, bool value) =>
        isFadingMenuField.SetValue(self, value);

    internal static MenuScreen? GetActiveMenuScreen(this UIManager self)
    {
        foreach (var field in menuScreenFields)
        {
            var screen = field.GetValue(self) as MenuScreen;
            if (screen == null || !screen.gameObject.activeInHierarchy)
                continue;

            return screen;
        }

        return null;
    }

    internal static void PlaySlider(this UIManager self) =>
        self
            .gameObject.FindChild("UIAudioPlayer")!
            .GetComponent<MenuAudioController>()!
            .PlaySlider();
}
