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

    internal static void FitToParentVertical(this RectTransform self, float anchorX = 0.5f)
    {
        self.anchoredPosition = Vector2.zero;
        self.anchorMin = new Vector2(anchorX, 0);
        self.anchorMax = new Vector2(anchorX, 1);
        self.sizeDelta = self.sizeDelta with { y = 0 };
    }

    internal static void FitToParentHorizontal(this RectTransform self, float anchorY = 0.5f)
    {
        self.anchoredPosition = Vector2.zero;
        self.anchorMin = new Vector2(0, anchorY);
        self.anchorMax = new Vector2(1, anchorY);
        self.sizeDelta = self.sizeDelta with { x = 0 };
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
