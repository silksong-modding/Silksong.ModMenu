using System;
using UnityEngine;

namespace Silksong.ModMenu.Internal;

internal class LateUpdateHelper : MonoBehaviour
{
    internal event Action? OnLateUpdate;

    private void LateUpdate() => OnLateUpdate?.Invoke();
}
