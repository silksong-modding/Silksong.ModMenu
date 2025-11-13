using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// Marker interface for a plugin that provides its own custom menu element.
/// </summary>
public interface IModMenuCustomElement : IModMenuInterface
{
    public SelectableElement BuildCustomElement();
}
