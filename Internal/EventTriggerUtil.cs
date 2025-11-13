using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal static class EventTriggerUtil
{
    internal static void SetCallback(this EventTrigger self, Action callback)
    {
        EventTrigger.TriggerEvent e = new();
        e.AddListener(_ => ExceptionUtil.Try(callback, "Button callback failed"));
        foreach (var entry in self.triggers)
            entry.callback = e;
    }

    internal static void SetCallback(this Slider self, Action<float> callback)
    {
        Slider.SliderEvent e = new();
        e.AddListener(v => ExceptionUtil.Try(() => callback(v), "Slider callback failed"));
        self.onValueChanged = e;
    }
}
