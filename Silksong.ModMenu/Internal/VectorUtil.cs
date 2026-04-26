using UnityEngine;

namespace Silksong.ModMenu.Internal;

internal static class VectorUtil
{
    // Using extension blocks with static methods so that usage mirrors the equivalent Mathf methods.

    extension(Vector2)
    {
        /// <summary>
        /// True if the vectors are approximately equal.
        /// See <see cref="Mathf.Approximately"/>.
        /// </summary>
        internal static bool Approximately(Vector2 v1, Vector2 v2) =>
            Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);

        /// <summary>
        /// Clamps all components of the vector to between 0 and 1.
        /// See <see cref="Mathf.Clamp01"/>.
        /// </summary>
        internal static Vector2 Clamp01(Vector2 v) => v.ClampVector2(Vector2.zero, Vector2.one);
    }

    extension(Vector3)
    {
        /// <inheritdoc cref="extension(Vector2).Approximately"/>
        internal static bool Approximately(Vector3 v1, Vector3 v2) =>
            Vector2.Approximately(v1, v2) && Mathf.Approximately(v1.z, v2.z);

        /// <inheritdoc cref="extension(Vector2).Clamp01"/>
        internal static Vector3 Clamp01(Vector3 v) =>
            ((Vector3)Vector2.Clamp01(v)) with
            {
                z = Mathf.Clamp01(v.z),
            };
    }
}
