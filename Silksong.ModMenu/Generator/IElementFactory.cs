using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Generator;

/// <summary>
/// Generator for a custom menu element, for the parameterized type.
/// </summary>
public interface IElementFactory<T, E> : IElementFactory
    where E : SelectableValueElement<T>
{
    /// <summary>
    /// Create a menu element for the parameterized type.
    /// </summary>
    public E CreateElement(LocalizedText name, LocalizedText description);
}

/// <summary>
/// Parent marker interface without generics.
/// </summary>
public interface IElementFactory { }
