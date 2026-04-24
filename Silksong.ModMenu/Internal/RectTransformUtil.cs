using System.Collections.Generic;
using Silksong.UnityHelper.Extensions;
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

    /// <summary>
    /// Gets the corners of a bounding box that encapsulates everything in
    /// <paramref name="transforms"/> and their descendants, relative to the
    /// coordinates of <paramref name="self"/>.
    /// </summary>
    internal static (Vector2 min, Vector2 max) GetRelativeBoundsOf(
        this RectTransform self,
        IEnumerable<Transform> transforms
    )
    {
        Vector2 min = Vector2.one * float.MaxValue,
            max = Vector2.one * float.MinValue;
        foreach (Transform item in transforms)
        {
            foreach (var (_, t) in item.EnumerateHierarchy())
            {
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(self, t);
                min = Vector2.Min(min, bounds.min);
                max = Vector2.Max(max, bounds.max);
            }
        }
        return (min, max);
    }

    /// <summary>
    /// True if <paramref name="self"/> is overlapping <paramref name="other"/>
    /// at all from the view of the <paramref name="camera"/> (or from the
    /// default UIManager camera, if none is specified).
    /// </summary>
    internal static bool Overlaps(
        this RectTransform self,
        RectTransform other,
        Camera? camera = null
    )
    {
        if (!camera)
            camera = UIManager.instance.UICanvas.worldCamera;

        if (self.rect.size.x > other.rect.size.x && self.rect.size.y > other.rect.size.y)
            (self, other) = (other, self);

        var corners = new Vector3[4];
        self.GetWorldCorners(corners);

        Vector3 max = corners[0],
            min = corners[0];

        for (int i = 1; i < 4; i++)
        {
            max = Vector3.Max(max, corners[i]);
            min = Vector3.Min(max, corners[i]);
        }
        Vector3 center = (max - min) / 2f + min,
            centerTop = new(center.x, max.y, center.z),
            centerBottom = new(center.x, min.y, center.z),
            centerLeft = new(min.x, center.y, center.z),
            centerRight = new(max.x, center.y, center.z);

        Vector3[] points = [centerTop, centerBottom, center, centerLeft, centerRight, .. corners];

        foreach (var point in points)
        {
            bool pointInView = RectTransformUtility.RectangleContainsScreenPoint(
                other,
                RectTransformUtility.WorldToScreenPoint(camera, point),
                camera
            );
            if (pointInView)
                return true;
        }
        return false;
    }
}
