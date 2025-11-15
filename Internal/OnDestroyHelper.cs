using System;
using UnityEngine;

namespace Silksong.ModMenu.Internal;

internal class OnDestroyHelper : MonoBehaviour
{
    internal event Action? Action;

    internal void OnDestroy() => Action?.Invoke();
}
