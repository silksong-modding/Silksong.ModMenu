using System;

namespace Silksong.ModMenu.Internal;

internal static class TypeUtil
{
    /// <summary>
    /// True if <paramref name="self"/> is descended from the raw generic type <paramref name="ancestor"/>.
    /// </summary>
    internal static bool IsSubclassOfRawGeneric(this Type self, Type ancestor)
    {
        while (self != typeof(object))
        {
            Type selfGeneric = self.IsGenericType ? self.GetGenericTypeDefinition() : self;
            if (ancestor == selfGeneric)
                return true;
            self = self.BaseType;
        }
        return false;
    }
}
