using UnityEngine;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Helper functions for IMenuEntities.
/// </summary>
public static class IMenuEntityExtensions
{
    /// <summary>
    /// Helper functions for IMenuEntities.
    /// </summary>
    extension(IMenuEntity self)
    {
        /// <summary>
        /// Shorthand for 'visible in hierarchy'
        /// </summary>
        public bool Visible => self.Visibility.VisibleInHierarchy;

        /// <summary>
        /// Convenience accessor for VisibleSelf.
        /// </summary>
        public bool VisibleSelf
        {
            get => self.Visibility.VisibleSelf;
            set => self.Visibility.VisibleSelf = value;
        }

        /// <summary>
        /// Convenience accessor for VisibileInHierarchy.
        /// </summary>
        public bool VisibleInHierarchy => self.Visibility.VisibleInHierarchy;

        /// <summary>
        /// Set the parent(s) of this entity, clearing any previous parents.
        /// </summary>
        public void SetParents(IMenuEntity parent, GameObject? container = null)
        {
            self.Visibility.SetParent(parent.Visibility);

            if (container != null)
                self.SetGameObjectParent(container);
            else
                self.ClearGameObjectParent();
        }

        /// <summary>
        /// Unset the parent(s) of this entity, which in most cases makes it invisible.
        /// </summary>
        public void ClearParents()
        {
            self.Visibility.ClearParent();
            self.ClearGameObjectParent();
        }
    }
}
