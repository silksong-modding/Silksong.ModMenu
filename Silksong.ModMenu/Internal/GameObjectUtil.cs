using System.Collections.Generic;
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

    internal static IEnumerable<Transform> WalkHierarchy(this Transform t)
    {
        yield return t;
        foreach (Transform child in t)
        {
            foreach (Transform t2 in child.WalkHierarchy())
                yield return t2;
        }
    }

    private class InactiveScope : System.IDisposable
    {
        private readonly bool prevActiveSelf;
        private readonly GameObject gameObject;

        internal InactiveScope(GameObject gameObject)
        {
            prevActiveSelf = gameObject.activeSelf;
            this.gameObject = gameObject;
            gameObject.SetActive(false);
        }

        public void Dispose() => gameObject.SetActive(prevActiveSelf);
    }

    internal static System.IDisposable TempInactive(this GameObject self) =>
        new InactiveScope(self);
}
