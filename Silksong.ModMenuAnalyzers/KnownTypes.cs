using System;
using Microsoft.CodeAnalysis;

namespace Silksong.ModMenuAnalyzers;

internal record KnownTypes(Compilation Compilation)
{
    public readonly ITypeSymbol ElementFactoryAttributeType =
        Compilation.GetTypeByMetadataName("Silksong.ModMenu.Generator.ElementFactoryAttribute`1")
        ?? throw new NullReferenceException(nameof(ElementFactoryAttributeType));
    public readonly ITypeSymbol IElementFactoryType =
        Compilation.GetTypeByMetadataName("Silksong.ModMenu.Generator.IElementFactory`2")
        ?? throw new NullReferenceException(nameof(IElementFactoryType));
    public readonly ITypeSymbol SubMenuAttributeType =
        Compilation.GetTypeByMetadataName("Silksong.ModMenu.Generator.SubMenuAttribute`1")
        ?? throw new NullReferenceException(nameof(SubMenuAttributeType));
}
