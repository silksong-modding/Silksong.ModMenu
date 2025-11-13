using UnityEngine;

namespace Silksong.ModMenu.Internal;

internal static class GameObjectUtil
{
    // TODO: Move this to UnityHelper and use that.
    internal static GameObject? FindChild(this GameObject self, string path)
    {
        var t = self.transform.Find(path);
        if (t == null)
            return null;

        return t.gameObject;
    }

    internal static void DestroyAllChildren(this GameObject self)
    {
        for (int i = self.transform.childCount - 1; i >= 0; i--)
        {
            var obj = self.transform.GetChild(i).gameObject;
            obj.transform.SetParent(null);
            Object.Destroy(obj);
        }
    }
}
