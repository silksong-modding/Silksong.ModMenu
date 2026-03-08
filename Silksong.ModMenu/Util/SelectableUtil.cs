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

    /// <summary>
    /// Navigation helper methods for Selectables.
    /// </summary>
    extension(Selectable self)
    {
        /// <summary>
        /// Set the neighbor upwards of this one to 'target'.
        /// </summary>
        public void SetNavUp(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnUp = target };

        /// <summary>
        /// Declare there is no neighbor upwards of this one.
        /// </summary>
        public void ClearNavUp() =>
            self.navigation = self.navigation.Explicit() with { selectOnUp = null };

        /// <summary>
        /// Set the neighbor leftwards of this one to 'target'.
        /// </summary>
        public void SetNavLeft(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnLeft = target };

        /// <summary>
        /// Declare there is no neighbor leftwards of this one.
        /// </summary>
        public void ClearNavLeft() =>
            self.navigation = self.navigation.Explicit() with { selectOnLeft = null };

        /// <summary>
        /// Set the neighbor rightwards of this one to 'target'.
        /// </summary>
        public void SetNavRight(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnRight = target };

        /// <summary>
        /// Declare there is no neighbor rightwards of this one.
        /// </summary>
        public void ClearNavRight() =>
            self.navigation = self.navigation.Explicit() with { selectOnRight = null };

        /// <summary>
        /// Set the neighbor downwards of this one to 'target'.
        /// </summary>
        public void SetNavDown(Selectable target) =>
            self.navigation = self.navigation.Explicit() with { selectOnDown = target };

        /// <summary>
        /// Declare there is no neightbor downwards of this one.
        /// </summary>
        public void ClearNavDown() =>
            self.navigation = self.navigation.Explicit() with { selectOnDown = null };

        /// <summary>
        /// Declare that this Selectable has no neighbors in any direction.
        /// </summary>
        public void ClearNav() =>
            self.navigation = new() { mode = Navigation.Mode.Explicit, wrapAround = false };
    }

    private static readonly int hideHash = Animator.StringToHash("hide");
    private static readonly int showHash = Animator.StringToHash("show");

    /// <summary>
    /// Force this Selectable to be selected by the active event system.
    /// </summary>
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

    /// <summary>
    /// Whether this Selectable is currently highlighted by the active event system.
    /// </summary>
    public static bool IsSelected(this Selectable self) =>
        EventSystem.current.currentSelectedGameObject == self.gameObject;
}
