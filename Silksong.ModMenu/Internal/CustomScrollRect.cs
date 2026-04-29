using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Identical to the regular <see cref="ScrollRect"/>, except it fixes the known bug
/// (See <see href="https://discussions.unity.com/t/934765">this Unity help forum post</see>)
/// where mousewheel/touchpad scrolling is always the opposite of what it should intuitively be for one scroll direction.
/// </summary>
internal class CustomScrollRect : ScrollRect
{
    public override void OnScroll(PointerEventData data)
    {
        if (horizontal)
        {
            if (vertical)
                data.scrollDelta *= new Vector2(-1, 1);
            else
                data.scrollDelta *= -1;
        }
        base.OnScroll(data);
    }
}
