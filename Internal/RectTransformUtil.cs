using UnityEngine;

namespace Silksong.ModMenu.Internal;

internal static class RectTransformUtil
{
    internal static void FitToParent(this RectTransform self)
    {
        self.anchoredPosition = Vector2.zero;
        self.anchorMin = Vector2.zero;
        self.anchorMax = Vector2.one;
        self.offsetMin = self.offsetMax = Vector2.zero;
        self.sizeDelta = Vector2.zero;
    }

    internal static void SetAnchoredPosition(this RectTransform self, Vector2 pos)
    {
        var size = self.rect.size;

        self.anchoredPosition = pos;
        self.anchorMin = self.anchorMax = new(0.5f, 0.5f);
        self.offsetMin = pos - size / 2;
        self.offsetMax = pos + size / 2;
        self.sizeDelta = size;
    }
}
