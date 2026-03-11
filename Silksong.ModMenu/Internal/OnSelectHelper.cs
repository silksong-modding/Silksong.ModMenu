using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silksong.ModMenu.Internal;

internal class OnSelectHelper : MonoBehaviour, ISelectHandler
{
    internal event Action? OnSelect;

    void ISelectHandler.OnSelect(BaseEventData eventData) => OnSelect?.Invoke();
}
