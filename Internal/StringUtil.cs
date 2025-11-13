using System.Text;

namespace Silksong.ModMenu.Internal;

internal static class StringUtil
{
    internal static string Truncate(this string self, int maxChars)
    {
        if (maxChars <= 3 || self.Length <= maxChars)
            return self;
        else
            return self[..(maxChars - 3)] + "...";
    }

    internal static string FirstLine(this string self, int maxChars)
    {
        var split = self.Split('\n')[0];
        split = split.Split("<br>")[0];
        if (split.Length > maxChars)
            return split = $"{split[..(maxChars - 3)]}...";
        else
            return split;
    }

    internal static string UnCamelCase(this string self)
    {
        // Don't change if it already has spaces.
        if (self.Contains(' '))
            return self;

        StringBuilder sb = new();

        bool prevUpper = false;
        bool first = true;
        foreach (var ch in self)
        {
            if (first)
            {
                first = false;
                sb.Append(char.ToUpper(ch));
                prevUpper = char.IsUpper(ch);
                continue;
            }

            if (char.IsUpper(ch) && !prevUpper)
                sb.Append(' ');
            sb.Append(ch);
            prevUpper = char.IsUpper(ch);
        }

        return sb.ToString();
    }
}
