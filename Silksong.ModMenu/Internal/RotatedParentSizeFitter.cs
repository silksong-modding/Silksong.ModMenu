using UnityEngine;
using UnityEngine.EventSystems;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Used to rotate a UI object 90 degrees and fit it to the size of its parent in one or both axes.
/// </summary>
/// <remarks>
/// RectTransform anchor stretching doesn't react to rotation. Which means, for example, if you rotate
/// a vertical slider 90 degrees to make a horizontal slider, you can't use its anchors to fit it to
/// the width of its parent. Which is where this component comes in.
/// </remarks>
[RequireComponent(typeof(RectTransform))]
internal class RotatedParentSizeFitter : UIBehaviour
{
    RectTransform RT,
        parentRT;
    public bool fitToWidth = false,
        fitToHeight = false;

    protected override void Awake()
    {
        base.Awake();
        RT = (RectTransform)transform;
        parentRT = (RectTransform)transform.parent;
    }

    protected void LateUpdate()
    {
        RT.rotation = Quaternion.Euler(0, 0, 90);
        RT.sizeDelta = new Vector2(
            fitToHeight ? parentRT.rect.height : RT.sizeDelta.x,
            fitToWidth ? parentRT.rect.width : RT.sizeDelta.y
        );
    }
}
