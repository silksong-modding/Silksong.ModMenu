namespace Silksong.ModMenu.Elements;

/// <summary>
/// Helper functions for IMenuEntities.
/// </summary>
public static class IMenuEntityExtensions
{
    extension(IMenuEntity self)
    {
        /// <summary>
        /// Shorthand for 'visible in hierarchy'
        /// </summary>
        public bool Visible => self.Visibility.VisibleInHierarchy;

        public bool VisibleSelf
        {
            get => self.Visibility.VisibleSelf;
            set => self.Visibility.VisibleSelf = value;
        }

        public bool VisibleInHierarchy => self.Visibility.VisibleInHierarchy;
    }
}
