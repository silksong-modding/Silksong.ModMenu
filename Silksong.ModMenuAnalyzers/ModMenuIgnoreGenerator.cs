using Microsoft.CodeAnalysis;

namespace Silksong.ModMenuAnalyzers;

/// <summary>
/// Generates a ModMenuIgnoreAttribute for internal use within a plugin, allowing ignoring classes and members
/// from menu generation without a runtime dependency on ModMenu
/// </summary>
[Generator(LanguageNames.CSharp)]
public class ModMenuIgnoreGenerator : IIncrementalGenerator
{
    private const string AttributeName = "ModMenuIgnoreAttribute";

    private const string Source = """
        namespace Silksong.ModMenu.Models.Generated;

        /// <summary>
        /// Opts a plugin class out of ModMenu's automatic menu generation.
        /// </summary>
        [global::System.AttributeUsage(
            global::System.AttributeTargets.Class
                | global::System.AttributeTargets.Field
                | global::System.AttributeTargets.Property,
            AllowMultiple = false
        )]
        internal sealed class ModMenuIgnoreAttribute : global::System.Attribute { }
        """;

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Skipped for ModMenu itself and for projects that already declare the attribute in the global namespace 
        var shouldGenerate = context.CompilationProvider.Select(
            (compilation, _) =>
                compilation.AssemblyName != "Silksong.ModMenu"
                && compilation.GetTypeByMetadataName(AttributeName) == null
        );

        context.RegisterSourceOutput(
            shouldGenerate,
            (ctx, generate) =>
            {
                if (generate)
                    ctx.AddSource("ModMenuIgnoreAttribute.g.cs", Source);
            }
        );
    }
}