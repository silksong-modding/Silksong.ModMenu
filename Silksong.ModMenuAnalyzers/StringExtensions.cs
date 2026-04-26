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
        string buffer = new(' ', spaces);
        return string.Join($"\n{buffer}", self);
    }
}
