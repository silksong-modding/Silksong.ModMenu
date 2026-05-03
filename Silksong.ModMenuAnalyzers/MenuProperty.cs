using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Silksong.ModMenuAnalyzers;

internal record MenuProperty(
    KnownTypes KnownTypes,
    ISymbol Symbol,
    IReadOnlyList<AttributeData> Attrs
)
{
    private string Name => Symbol.Name;

    private string DisplayName = "";
    private string Description = "";
    private readonly List<string> DefaultInitializer = [];
    private ITypeSymbol? SubMenuType;
    private ITypeSymbol? ElementFactoryType;

    private ITypeSymbol? ElementFactoryElementType =>
        ElementFactoryType?.GetInterfaceTypeArgument(KnownTypes.IElementFactoryType, 1);

    private ITypeSymbol DataType
    {
        get
        {
            if (Symbol is IFieldSymbol f)
                return f.Type;
            else if (Symbol is IPropertySymbol p)
                return p.Type;
            else
                throw new InvalidOperationException();
        }
    }

    private static bool IsRelevant(KnownTypes knownTypes, AttributeData data)
    {
        if (data.AttributeClass == null)
            return false;

        if (data.AttributeClass.Name == "ModMenuIgnoreAttribute")
            return true;
        if (data.AttributeClass.ToDisplayString() == "Silksong.ModMenu.Models.ModMenuNameAttribute")
            return true;
        if (
            data.AttributeClass.ToDisplayString()
            == "Silksong.ModMenu.Generator.ModMenuRangeAttribute"
        )
            return true;
        if (
            data.AttributeClass.ToDisplayString()
            == "Silksong.ModMenu.Generator.ModMenuIncludeAttribute"
        )
            return true;

        var origDef = data.AttributeClass.OriginalDefinition;
        if (SymbolEqualityComparer.Default.Equals(origDef, knownTypes.ElementFactoryAttributeType))
            return true;
        if (SymbolEqualityComparer.Default.Equals(origDef, knownTypes.SubMenuAttributeType))
            return true;

        return false;
    }

    private bool GetUniqueAttr(
        List<Diagnostic> diagnostics,
        Func<AttributeData, bool> matcher,
        out AttributeData? result
    )
    {
        List<AttributeData> matching = [.. Attrs.Where(matcher)];
        if (matching.Count > 1)
        {
            Diagnostics
                .ConflictingAttributes(Name)
                .Add(diagnostics, matching.Select(a => a.Location));
            result = null;
            return false;
        }

        result = matching.Count == 1 ? matching[0] : null;
        return true;
    }

    internal bool Init(List<Diagnostic> diagnostics)
    {
        var ignoredAttr = Attrs.FirstOrDefault(a =>
            a.AttributeClass?.Name == "ModMenuIgnoreAttribute"
        );
        var includedAttr = Attrs.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString()
            == "Silksong.ModMenu.Generator.ModMenuIncludeAttribute"
        );
        if (ignoredAttr != null && includedAttr != null)
        {
            Diagnostics
                .ConflictingAttributes(Name)
                .Add(diagnostics, [ignoredAttr.Location, includedAttr.Location]);
            return false;
        }

        bool ignored = ignoredAttr != null;
        bool included = includedAttr != null;
        bool isPublic = Symbol.DeclaredAccessibility == Accessibility.Public;
        if (ignored && !isPublic)
            Diagnostics.IgnoredNonPublicField.Add(diagnostics, ignoredAttr?.Location);
        if (included && isPublic)
            Diagnostics.IncludedPublicField.Add(diagnostics, includedAttr?.Location);

        bool visible = included || (isPublic && !ignored);
        if (!visible)
        {
            foreach (
                var attr in Attrs.Where(a =>
                    IsRelevant(KnownTypes, a)
                    && (!ignored || a.AttributeClass?.Name != "ModMenuIgnoreAttribute")
                )
            )
                Diagnostics
                    .IrrelevantAttribute(Name, attr.AttributeClass?.Name ?? "")
                    .Add(diagnostics, attr.Location);
            return false;
        }

        DisplayName =
            Attrs
                .Where(a =>
                    a.AttributeClass?.ToDisplayString()
                    == "Silksong.ModMenu.Models.ModMenuNameAttribute"
                )
                .Select(a => (string)a.ConstructorArguments[0].Value!)
                .FirstOrDefault()
            ?? Name.UnCamelCase();
        Description =
            Attrs
                .Where(a =>
                    a.AttributeClass?.ToDisplayString() == typeof(DescriptionAttribute).FullName
                )
                .Select(a =>
                    (string)a.NamedArgument(nameof(DescriptionAttribute.Description)).Value!
                )
                .FirstOrDefault()
            ?? "";

        if (
            !GetUniqueAttr(
                diagnostics,
                a => a.AttributeClass?.GetTypeArgument(KnownTypes.SubMenuAttributeType, 0) != null,
                out var subMenuAttr
            )
        )
            return false;
        SubMenuType = subMenuAttr?.AttributeClass?.GetTypeArgument(
            KnownTypes.SubMenuAttributeType,
            0
        );

        if (
            !GetUniqueAttr(
                diagnostics,
                a =>
                    a.AttributeClass?.GetTypeArgument(KnownTypes.ElementFactoryAttributeType, 0)
                    != null,
                out var elementFactoryAttr
            )
        )
            return false;
        ElementFactoryType = elementFactoryAttr?.AttributeClass?.GetTypeArgument(
            KnownTypes.ElementFactoryAttributeType,
            0
        );

        if (
            !GetUniqueAttr(
                diagnostics,
                a =>
                    a.AttributeClass?.ToDisplayString()
                    == "Silksong.ModMenu.Generator.ModMenuRangeAttribute",
                out var menuRangeAttr
            )
        )
            return false;

        int distinct =
            (subMenuAttr != null ? 1 : 0)
            + (elementFactoryAttr != null ? 1 : 0)
            + (menuRangeAttr != null ? 1 : 0);
        if (distinct > 1)
        {
            Diagnostics
                .ConflictingAttributes(Name)
                .Add(
                    diagnostics,
                    [subMenuAttr?.Location, elementFactoryAttr?.Location, menuRangeAttr?.Location]
                );
            return false;
        }

        if (SubMenuType == null && ElementFactoryType == null)
        {
            if (!ValidateModMenuRange(diagnostics, menuRangeAttr, out var bounds))
                return false;

            if (
                !InitNumericType(bounds)
                && !InitBoolType()
                && !InitKeyCodeType()
                && !InitEnumType()
                && !InitTextType()
            )
            {
                Diagnostics
                    .UnsupportedType(DataType.ToDisplayString())
                    .Add(diagnostics, Symbol.Locations);
                return false;
            }
        }

        return true;
    }

    private bool InitBoolType()
    {
        if (DataType.SpecialType != SpecialType.System_Boolean)
            return false;

        DefaultInitializer.Add(
            $@"{Name} = new Silksong.ModMenu.Elements.ChoiceElement<bool>({DisplayName.MakeLiteral()}, Silksong.ModMenu.Models.ChoiceModels.ForBool(), {Description.MakeLiteral()});"
        );
        return true;
    }

    private bool InitKeyCodeType()
    {
        if (DataType.ToDisplayString() != "UnityEngine.KeyCode")
            return false;

        DefaultInitializer.Add(
            $@"{Name} = new Silksong.ModMenu.Elements.KeyBindElement({DisplayName.MakeLiteral()});"
        );
        return true;
    }

    private bool InitEnumType()
    {
        if (DataType.TypeKind != TypeKind.Enum)
            return false;

        DefaultInitializer.Add(
            $@"{Name} = new Silksong.ModMenu.Elements.ChoiceElement<{DataType.ToDisplayString()}>({DisplayName.MakeLiteral()}, Silksong.ModMenu.Models.ChoiceModels.ForEnum<{DataType.ToDisplayString()}>(), {Description.MakeLiteral()});"
        );
        return true;
    }

    private bool ValidateModMenuRange(
        List<Diagnostic> diagnostics,
        AttributeData? rangeAttr,
        out (object min, object max)? bounds
    )
    {
        bounds = default;
        if (rangeAttr == null)
            return true;

        var info = NumericInfo.Get(DataType.SpecialType);
        if (info == null)
        {
            Diagnostics.ModMenuRangeTypeError.Add(diagnostics, rangeAttr?.Location);
            return false;
        }

        var min = rangeAttr.NamedArgument("Min", 0).Value;
        var max = rangeAttr.NamedArgument("Max", 1).Value;
        var minTyped = info.ConvertFn(min);
        var maxTyped = info.ConvertFn(max);
        if (minTyped == null || maxTyped == null)
        {
            Diagnostics.ModMenuRangeBoundError(info.TypeName).Add(diagnostics, rangeAttr?.Location);
            return false;
        }

        if (info.CompareToFn(min, max) > 0)
        {
            Diagnostics.ModMenuRangeInvertedError.Add(diagnostics, rangeAttr?.Location);
            return false;
        }

        return true;
    }

    private bool InitNumericType((object min, object max)? range)
    {
        var info = NumericInfo.Get(DataType.SpecialType);
        if (info == null)
            return false;

        var bounds = range.HasValue
            ? $@"{info.PrintFn(range.Value.min)}, {info.PrintFn(range.Value.max)}"
            : "";
        DefaultInitializer.Add(
            $@"{Name} = new Silksong.ModMenu.Elements.TextInput<{info.TypeName}>({DisplayName.MakeLiteral()}, Silksong.ModMenu.Models.TextModels.{info.FactoryName}({bounds}), {Description.MakeLiteral()});"
        );
        return true;
    }

    private bool InitTextType()
    {
        if (DataType.SpecialType != SpecialType.System_String)
            return false;

        DefaultInitializer.Add(
            $@"{Name} = new Silksong.ModMenu.Elements.TextInput<string>({DisplayName.MakeLiteral()}, Silksong.ModMenu.Models.TextModels.ForStrings(), {Description.MakeLiteral()});"
        );
        return true;
    }

    internal string DefineProperty()
    {
        string type;
        string subMenu = "";
        string privateName = $@"_{Name}";
        if (SubMenuType != null)
        {
            type =
                $@"Silksong.ModMenu.Generator.SubMenuElement<{DataType.ToDisplayString()}, {SubMenuType.ToDisplayString()}>";
            subMenu = ".SubMenu";
        }
        else if (ElementFactoryType != null)
            type = ElementFactoryElementType!.ToDisplayString();
        else
            type =
                $@"Silksong.ModMenu.Elements.SelectableValueElement<{DataType.ToDisplayString()}>";

        return $$"""
            public {{type}} {{Name}}
            {
                get => _{{Name}};
                set
                {
                    if (value == null) throw new System.ArgumentNullException(nameof({{Name}}));
                    if ({{privateName}} == value) return;

                    if ({{privateName}} != null)
                        {{privateName}}{{subMenu}}.OnValueChanged -= {{SubscriberName}};
                    {{privateName}} = value;
                    {{privateName}}{{subMenu}}.OnValueChanged += {{SubscriberName}};
                }
            }
            private {{type}} {{privateName}};
            """;
    }

    internal IEnumerable<string> Initialize()
    {
        List<string> lines = [];
        if (SubMenuType != null)
        {
            lines.Add($@"{SubscriberName} = _ => InvokeValueChanged(new(nameof({Name}), null));");
            lines.Add(
                $@"{Name} = new Silksong.ModMenu.Generator.SubMenuElement<{DataType.ToDisplayString()}, {SubMenuType.ToDisplayString()}>({DisplayName.MakeLiteral()}, new {SubMenuType.ToDisplayString()}(), {Description.MakeLiteral()});"
            );
        }
        else
        {
            lines.Add(
                $@"{SubscriberName} = value => InvokeValueChanged(new(nameof({Name}), value));"
            );
            if (ElementFactoryType != null)
                lines.Add(
                    $@"{Name} = new {ElementFactoryType.ToDisplayString()}().CreateElement({DisplayName.MakeLiteral()}, {Description.MakeLiteral()});"
                );
            else
                lines.AddRange(DefaultInitializer);
        }

        return lines;
    }

    internal string ExportTo()
    {
        if (SubMenuType != null)
            return $@"{Name}.SubMenu.ExportTo(data.{Name});";
        else
            return $@"data.{Name} = {Name}.Value;";
    }

    internal string ApplyFrom()
    {
        if (SubMenuType != null)
            return $@"{Name}.SubMenu.ApplyFrom(data.{Name});";
        else
            return $@"{Name}.Value = data.{Name};";
    }

    internal string YieldElement() => $"yield return {Name};";

    private string SubscriberName => $"_{Name}_subscriber";

    internal string DefineSubscriber()
    {
        string type = DataType.ToDisplayString();
        if (SubMenuType != null)
            type = "Silksong.ModMenu.Generator.CustomMenuValueChangedEvent";

        return $@"private readonly System.Action<{type}> {SubscriberName};";
    }
}
