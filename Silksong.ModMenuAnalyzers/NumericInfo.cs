using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Silksong.ModMenuAnalyzers;

internal record NumericInfo(
    string TypeName,
    string FactoryName,
    Func<object?, object?> ConvertFn,
    Func<object?, object?, int> CompareToFn,
    Func<object, string> PrintFn
)
{
    internal static NumericInfo? Get(SpecialType specialType) =>
        specialType switch
        {
            SpecialType.System_SByte => Create(
                "ForSignedBytes",
                Convert.ToSByte,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_Byte => Create(
                "ForBytes",
                Convert.ToByte,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_Int16 => Create(
                "ForShorts",
                Convert.ToInt16,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_UInt16 => Create(
                "ForUnsignedShorts",
                Convert.ToUInt16,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_Int32 => Create(
                "ForIntegers",
                Convert.ToInt32,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_UInt32 => Create(
                "ForUnsignedIntegers",
                Convert.ToUInt32,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_Int64 => Create(
                "ForLongs",
                Convert.ToInt64,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_UInt64 => Create(
                "ForUnsignedLongs",
                Convert.ToUInt64,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_Single => Create(
                "ForFloats",
                Convert.ToSingle,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            SpecialType.System_Double => Create(
                "ForDoubles",
                Convert.ToDouble,
                (a, b) => a.CompareTo(b),
                v => SyntaxFactory.Literal(v)
            ),
            _ => null,
        };

    private static NumericInfo Create<T>(
        string factoryName,
        Func<object, T> convertFn,
        Func<T, T, int> compareFn,
        Func<T, SyntaxToken> literalFn
    ) =>
        new(
            TypeName: typeof(T).FullName,
            FactoryName: factoryName,
            ConvertFn: o =>
            {
                if (o == null)
                    return null;

                try
                {
                    return convertFn(o);
                }
                catch (Exception)
                {
                    return null;
                }
            },
            CompareToFn: (a, b) => compareFn(convertFn(a!)!, convertFn(b!)),
            PrintFn: o =>
                SyntaxFactory
                    .LiteralExpression(SyntaxKind.NumericLiteralExpression, literalFn(convertFn(o)))
                    .ToFullString()
        );
}
