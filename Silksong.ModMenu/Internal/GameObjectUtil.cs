using UnityEngine;

namespace Silksong.ModMenu.Internal;

internal static class GameObjectUtil
{
    extension(GameObject self)
    {
        internal void DestroyAllChildren()
        {
            for (int i = self.transform.childCount - 1; i >= 0; i--)
            {
                var obj = self.transform.GetChild(i).gameObject;
                obj.transform.SetParent(null);
                Object.Destroy(obj);
            }
        }

        /// <summary>
        /// Casts the object's <c>transform</c> to a <see cref="RectTransform"/>.
        /// </summary>
        /// <exception cref="System.InvalidCastException">
        ///     If the object doesn't actually have a RectTransform.
        /// </exception>
        internal RectTransform RectTransform => (RectTransform)self.transform;
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
