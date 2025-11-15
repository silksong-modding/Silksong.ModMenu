using UnityEngine;

namespace Silksong.ModMenu.Internal;

internal static class GameObjectUtil
{
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
