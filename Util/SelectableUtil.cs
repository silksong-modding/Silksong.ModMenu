using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Util;

/// <summary>
/// Helpers for working with Selectables and navigation.
/// </summary>
public static class SelectableUtil
{
    private static Navigation Explicit(this Navigation self) =>
        self with
        {
            mode = Navigation.Mode.Explicit,
            wrapAround = false,
        };

    extension(Selectable self)
    {
        public void SetNavUp(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnUp = target };

        public void ClearNavUp() =>
            self.navigation = self.navigation.Explicit() with { selectOnUp = null };

        public void SetNavLeft(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnLeft = target };

        public void ClearNavLeft() =>
            self.navigation = self.navigation.Explicit() with { selectOnLeft = null };

        public void SetNavRight(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnRight = target };

        public void ClearNavRight() =>
            self.navigation = self.navigation.Explicit() with { selectOnRight = null };

        public void SetNavDown(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnDown = target };

        public void ClearNavDown() =>
            self.navigation = self.navigation.Explicit() with { selectOnDown = null };

        public void ClearNav() =>
            self.navigation = new() { mode = Navigation.Mode.Explicit, wrapAround = false };
    }

    private static readonly int hideHash = Animator.StringToHash("hide");
    private static readonly int showHash = Animator.StringToHash("show");

    public static void ForceSelect(this Selectable self)
    {
        // Mostly copied from MenuButtonList.
        UIManager.HighlightSelectableNoSound(self);
        foreach (var animator in self.GetComponentsInChildren<Animator>())
        {
            if (animator.HasParameter(hideHash))
                animator.ResetTrigger(hideHash);
            if (animator.HasParameter(showHash))
                animator.SetTrigger(showHash);
        }
    }

    public static bool IsSelected(this Selectable self) =>
        EventSystem.current.currentSelectedGameObject == self.gameObject;
}
