using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Models;

/// <summary>
/// Converts 'item' to string with the additional context of an associated index, most likely within a list.
/// </summary>
public delegate LocalizedText IndexedToString<T>(int index, T item);
