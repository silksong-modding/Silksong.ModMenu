using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// A component which invokes an event every time the object it's attached to has a
/// child added/removed/reordered, and when any child's position changes.
/// </summary>
[RequireComponent(typeof(RectTransform))]
internal class OnChildTransformsChangeHelper : UIBehaviour
{
    public event Action? OnChildrenChanged;
    readonly Dictionary<Transform, Vector2> childPositions = [];

    protected void OnTransformChildrenChanged()
    {
        foreach (Transform child in childPositions.Keys.ToList())
        {
            if (child.parent != transform)
                childPositions.Remove(child);
        }
        OnChildrenChanged?.Invoke();
    }

    protected void Update()
    {
        bool doInvoke = false;
        foreach (Transform child in transform)
        {
            var childRT = (RectTransform)child;
            if (
                childPositions.TryGetValue(child, out var oldPos)
                && !Vector2.Approximately(oldPos, childRT.anchoredPosition)
            )
            {
                doInvoke = true;
            }
            childPositions[child] = childRT.anchoredPosition;
        }
        if (doInvoke)
            OnChildrenChanged?.Invoke();
    }
}
