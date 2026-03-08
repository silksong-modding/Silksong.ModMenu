using System;

namespace Silksong.ModMenu.Internal;

internal static class ExceptionUtil
{
    internal static bool Try(Action cb, string header)
    {
        try
        {
            cb();
            return true;
        }
        catch (Exception e)
        {
            ModMenuPlugin.LogError($"{header}: {e}");
            return false;
        }
    }
}
