using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Silksong.ModMenuAnalyzers;

internal static class StringExtensions
{
    internal static string MakeLiteral(this string self) =>
        SyntaxFactory.Literal(self).ToFullString();

    internal static string UnCamelCase(this string self)
    {
        // Don't change if it already has spaces.
        if (self.Contains(" "))
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

    internal static string JoinIndented(this IEnumerable<string> self, int spaces)
    {
        string separator = $"\n{new string(' ', spaces)}";

        // Indent all non-empty lines except the first one.
        StringBuilder sb = new();
        bool first = true;
        foreach (var s in self)
        {
            foreach (var line in s.Split('\n'))
            {
                if (first)
                {
                    sb.Append(line);
                    first = false;
                }
                else if (line.Trim().Length == 0)
                    sb.Append('\n');
                else
                {
                    sb.Append(separator);
                    sb.Append(line);
                }
            }
        }

        return sb.ToString();
    }
}
