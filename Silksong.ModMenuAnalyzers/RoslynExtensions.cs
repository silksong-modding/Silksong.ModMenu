using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Silksong.ModMenuAnalyzers;

internal static class RoslynExtensions
{
    extension(AttributeData self)
    {
        internal Location? Location => self.ApplicationSyntaxReference?.GetSyntax().GetLocation();

        internal TypedConstant NamedArgument(string name, int position = 0)
        {
            List<TypedConstant> named =
            [
                .. self.NamedArguments.Where(a => a.Key == name).Select(a => a.Value),
            ];
            return named.Count > 0 ? named[0] : self.ConstructorArguments[position];
        }
    }

    extension(INamedTypeSymbol self)
    {
        internal IEnumerable<INamedTypeSymbol> BaseTypes
        {
            get
            {
                INamedTypeSymbol? type = self;
                while (type != null && type.SpecialType != SpecialType.System_Object)
                {
                    yield return type;
                    type = type.BaseType;
                }
            }
        }

        internal IEnumerable<(ISymbol symbol, IReadOnlyList<AttributeData> attrs)> WalkMembers()
        {
            Dictionary<ISymbol, List<AttributeData>> attributes = [];
            List<ISymbol> order = [];
            foreach (var type in self.BaseTypes.Reverse())
            {
                foreach (var member in type.GetMembers())
                {
                    switch (member)
                    {
                        case IFieldSymbol field:
                            attributes.Add(field, [.. field.GetAttributes()]);
                            order.Add(field);
                            break;
                        case IPropertySymbol property:
                            var attrs = property.GetAttributes();
                            if (
                                property.BaseProperties.FirstOrDefault(attributes.ContainsKey)
                                is IPropertySymbol existing
                            )
                                attributes[existing].AddRange(attrs);
                            else
                            {
                                attributes.Add(property, [.. attrs]);
                                order.Add(property);
                            }
                            break;
                    }
                }
            }

            return order.Select(p => (p, (IReadOnlyList<AttributeData>)attributes[p]));
        }

        internal ITypeSymbol? GetTypeArgument(ITypeSymbol baseType, int index) =>
            SymbolEqualityComparer.Default.Equals(self.OriginalDefinition, baseType)
                ? self.TypeArguments[index]
                : null;
    }

    extension(IPropertySymbol self)
    {
        internal IEnumerable<IPropertySymbol> BaseProperties
        {
            get
            {
                IPropertySymbol? property = self;
                while (property != null)
                {
                    yield return property;
                    property = property.OverriddenProperty;
                }
            }
        }
    }

    extension(ITypeSymbol self)
    {
        internal ITypeSymbol? GetInterfaceTypeArgument(ITypeSymbol baseType, int index) =>
            self
                .AllInterfaces.Select(i => i.GetTypeArgument(baseType, index))
                .FirstOrDefault(i => i != null);
    }

    extension(TypedConstant self)
    {
        internal bool MakeLiteral(out string literal)
        {
            literal = "";
            if (self.IsNull)
            {
                literal = "null";
                return true;
            }

            switch (self.Kind)
            {
                case TypedConstantKind.Primitive:
                    return LiteralFromPrimitive(self.Value, out literal);
                case TypedConstantKind.Enum:
                {
                    if (self.Value is IFieldSymbol field)
                        literal = $"{self.Type!.ToDisplayString()}.{field.Name}";
                    else if (FindMatchingEnum(self.Value, self.Type!, out var name))
                        literal = $"{self.Type!.ToDisplayString()}.{name}";
                    else if (LiteralFromPrimitive(self.Value, out var repr))
                        literal = $"({self.Type!.ToDisplayString()}){repr}";
                    else
                        return false;

                    return true;
                }
                default:
                    return false;
            }

            static bool LiteralFromPrimitive(object? o, out string repr)
            {
                repr = "";
                if (o is bool b)
                {
                    repr = b ? "true" : "false";
                    return true;
                }

                SyntaxToken? token = o switch
                {
                    sbyte value => Literal(value),
                    byte value => Literal(value),
                    short value => Literal(value),
                    ushort value => Literal(value),
                    int value => Literal(value),
                    uint value => Literal(value),
                    long value => Literal(value),
                    ulong value => Literal(value),
                    float value => Literal(value),
                    double value => Literal(value),
                    char value => Literal(value),
                    string value => Literal(value),
                    _ => null,
                };
                if (token == null)
                    return false;

                repr = token.Value.ToFullString();
                return true;
            }

            static bool FindMatchingEnum(object? value, ITypeSymbol type, out string name)
            {
                var match = type.GetMembers()
                    .OfType<IFieldSymbol>()
                    .FirstOrDefault(f =>
                        !f.IsImplicitlyDeclared
                        && f.HasConstantValue
                        && Equals(value, f.ConstantValue)
                    );

                name = match?.Name ?? "";
                return match != null;
            }
        }
    }
}
