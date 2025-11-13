using System.Reflection;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal static class MenuOptionHorizontalUtil
{
    private static readonly FieldInfo uiAudioPlayerField = typeof(MenuOptionHorizontal).GetField(
        "uiAudioPlayer",
        BindingFlags.NonPublic | BindingFlags.Instance
    );

    internal static void PlaySlider(this MenuOptionHorizontal self)
    {
        var controller = uiAudioPlayerField.GetValue(self) as MenuAudioController;
        controller?.PlaySlider();
    }
}
