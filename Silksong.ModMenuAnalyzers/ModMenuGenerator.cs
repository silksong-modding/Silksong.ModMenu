using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Silksong.ModMenuAnalyzers;

/// <summary>
/// Generates a custom ModMenu class for an annotated data type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class ModMenuGenerator : IIncrementalGenerator
{
    private const string VERSION = "1.0.0";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO: Collect MenuProperties directly instead of the entire class syntax, for better incrementality. Requires making MenuProperties cacheable.
        var symbols = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Silksong.ModMenu.Generator.GenerateMenuAttribute",
                (node, _) => node is ClassDeclarationSyntax cds,
                (ctx, _) => (ctx.SemanticModel.Compilation, ctx.TargetSymbol as INamedTypeSymbol)
            )
            .Where(p => p.Item2 != null);

        context.RegisterSourceOutput(
            symbols,
            (ctx, pair) =>
            {
                var (compilation, classSymbol) = pair;
                List<Diagnostic> diagnostics = [];
                if (GenerateCustomMenu(compilation, classSymbol!, diagnostics, out var source))
                    ctx.AddSource(
                        $"{classSymbol!.Name}Menu.g.cs",
                        SourceText.From(source, Encoding.UTF8)
                    );

                foreach (var diagnostic in diagnostics)
                    ctx.ReportDiagnostic(diagnostic);
            }
        );
    }

    private static bool GenerateCustomMenu(
        Compilation compilation,
        INamedTypeSymbol classSymbol,
        List<Diagnostic> diagnostics,
        out string source
    )
    {
        try
        {
            return GenerateCustomMenuImpl(compilation, classSymbol, diagnostics, out source);
        }
        catch (Exception ex)
        {
            Diagnostics.AnalyzerFailure(ex).Add(diagnostics, classSymbol.Locations);
            source = "";
            return false;
        }
    }

    private static bool GenerateCustomMenuImpl(
        Compilation compilation,
        INamedTypeSymbol classSymbol,
        List<Diagnostic> diagnostics,
        out string source
    )
    {
        KnownTypes knownTypes = new(compilation);

        source = "";
        if (classSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            Diagnostics.DataClassNotPublic.Add(diagnostics, classSymbol.Locations);
            return false;
        }

        List<MenuProperty> properties =
        [
            .. classSymbol
                .WalkMembers()
                .Select(pair => new MenuProperty(knownTypes, pair.symbol, pair.attrs))
                .Where(prop => prop.Init(diagnostics)),
        ];
        bool empty = properties.Count == 0;
        if (empty)
            Diagnostics.DataClassEmpty(classSymbol.Name).Add(diagnostics, classSymbol.Locations);

        string className = classSymbol.ToDisplayString();
        string menuClassName = $"{classSymbol.Name}Menu";

        string propertyDefinitions = properties.Select(p => p.DefineProperty()).JoinIndented(4);
        string constructorBody = properties.SelectMany(p => p.Initialize()).JoinIndented(8);
        string exportBody = properties.Select(p => p.ExportTo()).JoinIndented(8);
        string applyBody = properties.Select(p => p.ApplyFrom()).JoinIndented(12);
        string elementsBody = empty
            ? "return [];"
            : properties.Select(p => p.YieldElement()).JoinIndented(8);
        string subscriberDefinitions = properties.Select(p => p.DefineSubscriber()).JoinIndented(4);

        source = $$"""
            #nullable enable            

            namespace {{classSymbol.ContainingNamespace.ToDisplayString()}};

            /// Custom menu class generated for {{className}}.
            [System.CodeDom.Compiler.GeneratedCode("ModMenuGenerator", {{VERSION.MakeLiteral()}})]
            public class {{menuClassName}} : Silksong.ModMenu.Generator.ICustomMenu<{{className}}>
            {
                {{propertyDefinitions}}

                /// An aggregate event notified whenever any menu element in this class has its value changed.
                public event System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent>? OnValueChanged;

                public {{menuClassName}}()
                {
                    {{constructorBody}}
                }

                /// <inheritdoc />
                public void ExportTo({{className}} data)
                {
                    {{exportBody}}
                }

                /// <inheritdoc />
                public void ApplyFrom({{className}} data)
                {
                    using (notifySubscribers.Suppress())
                    {
                        {{applyBody}}
                    }
                }

                /// <inheritdoc />
                public System.Collections.Generic.IEnumerable<Silksong.ModMenu.Elements.MenuElement> Elements()
                {
                    {{elementsBody}}
                }

                private readonly Silksong.ModMenu.Util.EventSuppressor notifySubscribers = new();

                private void InvokeValueChanged(Silksong.ModMenu.Generator.CustomMenuValueChangedEvent args)
                {
                    if (notifySubscribers.Suppressed) return;
                    OnValueChanged?.Invoke(args);
                }

                {{subscriberDefinitions}}
            }
            """;
        return diagnostics.All(d => d.Descriptor.DefaultSeverity != DiagnosticSeverity.Error);
    }
}
