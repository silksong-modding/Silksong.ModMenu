using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

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
}
