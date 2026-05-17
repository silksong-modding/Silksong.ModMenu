using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Silksong.ModMenuAnalyzers;

internal record DiagnosticDescriptorWrapper(
    DiagnosticDescriptor Descriptor,
    IReadOnlyList<object?> Args
)
{
    public readonly IReadOnlyList<object?> Args = [.. Args];

    internal void Add(List<Diagnostic> sink, Location? location) => Add(sink, [location]);

    internal void Add(List<Diagnostic> sink, IEnumerable<Location?> locations) =>
        sink.AddRange(
            locations
                .Where(loc => loc != null)
                .DefaultIfEmpty()
                .Select(loc => Diagnostic.Create(Descriptor, loc, [.. Args]))
        );
}

internal static class Diagnostics
{
    private const string CATEGORY = "SilksongModMenu";

    private static readonly DiagnosticDescriptor dataClassNotPublic = new(
        id: "SSMM0001",
        title: "Mod Menu data classes must be public",
        messageFormat: "Mod Menu data classes must have public visibility",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    internal static DiagnosticDescriptorWrapper DataClassNotPublic => new(dataClassNotPublic, []);

    private static readonly DiagnosticDescriptor dataClassEmpty = new(
        id: "SSMM0002",
        title: "Data class has no public fields",
        messageFormat: "{0} has no public fields or properties, so the generated menu will be empty",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper DataClassEmpty(string className) =>
        new(dataClassEmpty, [className]);

    private static readonly DiagnosticDescriptor ignoredNonPublicField = new(
        id: "SSMM0003",
        title: "Non-public fields and properties are ignored by default",
        messageFormat: "Non-public fields and properties are ignored by default",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    internal static DiagnosticDescriptorWrapper IgnoredNonPublicField =>
        new(ignoredNonPublicField, []);

    private static readonly DiagnosticDescriptor irrelevantAttribute = new(
        id: "SSMM0004",
        title: "Irrelevant attribute",
        messageFormat: "{0} is not generated so attribute {1} is irrelevant",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper IrrelevantAttribute(
        string fieldName,
        string attrClass
    ) => new(irrelevantAttribute, [fieldName, attrClass]);

    private static readonly DiagnosticDescriptor conflictingAttributes = new(
        id: "SSMM0005",
        title: "Conflicting attribute",
        messageFormat: "Two or more attributes on {0} are in conflict",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper ConflictingAttributes(string fieldName) =>
        new(conflictingAttributes, [fieldName]);

    private static readonly DiagnosticDescriptor unsupportedType = new(
        id: "SSMM0006",
        title: "Unsupported type",
        messageFormat: "GenerateMenu does not support elements of type {0}.  Ignore this field, privatize it, or add an [ElementFactory] attribute.",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper UnsupportedType(string type) =>
        new(unsupportedType, [type]);

    private static readonly DiagnosticDescriptor analyzerFailure = new(
        id: "SSMM0007",
        title: "Analyzer Error",
        messageFormat: "ModMenuGenerator failed with an internal error: {0}",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper AnalyzerFailure(Exception ex) =>
        new(analyzerFailure, [ex]);

    private static readonly DiagnosticDescriptor modMenuRangeTypeError = new(
        id: "SSMM0008",
        title: "Mod Menu Range Type Error",
        messageFormat: "ModMenuRange can only be applied to numeric properties",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper ModMenuRangeTypeError =>
        new(modMenuRangeTypeError, []);

    private static readonly DiagnosticDescriptor modMenuRangeBoundError = new(
        id: "SSMM0009",
        title: "Mod Menu Range Bound Error",
        messageFormat: "ModMenuRange bounds could not be converted to {0}",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper ModMenuRangeBoundError(string typeName) =>
        new(modMenuRangeBoundError, [typeName]);

    private static readonly DiagnosticDescriptor modMenuRangeInvertedError = new(
        id: "SSMM0010",
        title: "Mod Menu Range Inverted Error",
        messageFormat: "ModMenuRange minimum must be <= to maximum",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper ModMenuRangeInvertedError =>
        new(modMenuRangeInvertedError, []);

    private static readonly DiagnosticDescriptor includedPublicField = new(
        id: "SSMM0011",
        title: "Unnecessary ModMenuInclude",
        messageFormat: "Public fields and properties are included by default",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper IncludedPublicField => new(includedPublicField, []);

    private static readonly DiagnosticDescriptor invalidOptions = new(
        id: "SSMM0012",
        title: "Invalid ModMenuOptions",
        messageFormat: "Could not handle ModMenuOptions argument",
        category: CATEGORY,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static DiagnosticDescriptorWrapper InvalidOptions => new(invalidOptions, []);
}
