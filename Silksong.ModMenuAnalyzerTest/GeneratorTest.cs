using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Silksong.ModMenu.Generator;
using Silksong.ModMenuAnalyzers;

namespace Silksong.ModMenuAnalyzerTest;

public class SnapshotTest
{
    [Fact]
    public async Task TestBasicGeneration()
    {
        string source = /*lang=c#-test*/
            """
            namespace Test;

            [Silksong.ModMenu.Generator.GenerateMenu]
            public class TestData
            {
               public int MyInt = 2;
            }
            """;

        string gen = /*lang=c#-test*/
            """
            #nullable enable            

            namespace Test;

            /// Custom menu class generated for Test.TestData.
            [System.CodeDom.Compiler.GeneratedCode("ModMenuGenerator", "1.0.0")]
            public class TestDataMenu : Silksong.ModMenu.Generator.ICustomMenu<Test.TestData>
            {
                public Silksong.ModMenu.Elements.SelectableValueElement<int> MyInt
                {
                    get => field;
                    set
                    {
                        if (field == value) return;
                        if (value == null) throw new System.ArgumentNullException(nameof(MyInt));

                        field?.OnValueChanged -= __MyInt_subscriber;
                        field = value;
                        field.OnValueChanged += __MyInt_subscriber;
                    }
                }

                /// An aggregate event notified whenever any menu element in this class has its value changed.
                public event System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent>? OnValueChanged;

                public TestDataMenu()
                {
                    __MyInt_subscriber = value => InvokeValueChanged(new(nameof(MyInt), value));
                    MyInt = new Silksong.ModMenu.Elements.TextInput<System.Int32>("My Int", Silksong.ModMenu.Models.TextModels.ForIntegers(), "");
                }

                /// <inheritdoc />
                public void ExportTo(Test.TestData data)
                {
                    data.MyInt = MyInt.Value;
                }

                /// <inheritdoc />
                public void ApplyFrom(Test.TestData data)
                {
                    using (notifySubscribers.Suppress())
                    {
                        MyInt.Value = data.MyInt;
                    }
                }

                /// <inheritdoc />
                public System.Collections.Generic.IEnumerable<Silksong.ModMenu.Elements.MenuElement> Elements()
                {
                    yield return MyInt;
                }

                private readonly Silksong.ModMenu.Util.EventSuppressor notifySubscribers = new();

                private void InvokeValueChanged(Silksong.ModMenu.Generator.CustomMenuValueChangedEvent args)
                {
                    if (notifySubscribers.Suppressed) return;
                    OnValueChanged?.Invoke(args);
                }

                private readonly System.Action<int> __MyInt_subscriber;
            }
            """;

        await ExpectSourceCode(source, ("TestDataMenu.g.cs", gen));
    }

    [Fact]
    public async Task TestSubMenuGeneration()
    {
        string source = /*lang=c#-test*/
            """
            namespace Test;

            [Silksong.ModMenu.Generator.GenerateMenu]
            public class TestData
            {
               [Silksong.ModMenu.Generator.SubMenu<SubDataMenu>]
               public SubData SubData = new();
            }

            [Silksong.ModMenu.Generator.GenerateMenu]
            public class SubData
            {
                public string MyString = "foo";
            }
            """;

        string gen1 = /*lang=c#-test*/
            """
            #nullable enable            

            namespace Test;

            /// Custom menu class generated for Test.TestData.
            [System.CodeDom.Compiler.GeneratedCode("ModMenuGenerator", "1.0.0")]
            public class TestDataMenu : Silksong.ModMenu.Generator.ICustomMenu<Test.TestData>
            {
                public Silksong.ModMenu.Generator.SubMenuElement<Test.SubData, SubDataMenu> SubData
                {
                    get => field;
                    set
                    {
                        if (field == value) return;
                        if (value == null) throw new System.ArgumentNullException(nameof(SubData));

                        field?.SubMenu.OnValueChanged -= __SubData_subscriber;
                        field = value;
                        field.SubMenu.OnValueChanged += __SubData_subscriber;
                    }
                }

                /// An aggregate event notified whenever any menu element in this class has its value changed.
                public event System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent>? OnValueChanged;

                public TestDataMenu()
                {
                    __SubData_subscriber = _ => InvokeValueChanged(new(nameof(SubData), null));
                    SubData = new Silksong.ModMenu.Generator.SubMenuElement<Test.SubData, SubDataMenu>("Sub Data", new SubDataMenu(), "");
                }

                /// <inheritdoc />
                public void ExportTo(Test.TestData data)
                {
                    SubData.SubMenu.ExportTo(data.SubData);
                }

                /// <inheritdoc />
                public void ApplyFrom(Test.TestData data)
                {
                    using (notifySubscribers.Suppress())
                    {
                        SubData.SubMenu.ApplyFrom(data.SubData);
                    }
                }

                /// <inheritdoc />
                public System.Collections.Generic.IEnumerable<Silksong.ModMenu.Elements.MenuElement> Elements()
                {
                    yield return SubData;
                }

                private readonly Silksong.ModMenu.Util.EventSuppressor notifySubscribers = new();

                private void InvokeValueChanged(Silksong.ModMenu.Generator.CustomMenuValueChangedEvent args)
                {
                    if (notifySubscribers.Suppressed) return;
                    OnValueChanged?.Invoke(args);
                }

                private readonly System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent> __SubData_subscriber;
            }
            """;

        string gen2 = /*lang=c#-test*/
            """
            #nullable enable            

            namespace Test;

            /// Custom menu class generated for Test.SubData.
            [System.CodeDom.Compiler.GeneratedCode("ModMenuGenerator", "1.0.0")]
            public class SubDataMenu : Silksong.ModMenu.Generator.ICustomMenu<Test.SubData>
            {
                public Silksong.ModMenu.Elements.SelectableValueElement<string> MyString
                {
                    get => field;
                    set
                    {
                        if (field == value) return;
                        if (value == null) throw new System.ArgumentNullException(nameof(MyString));

                        field?.OnValueChanged -= __MyString_subscriber;
                        field = value;
                        field.OnValueChanged += __MyString_subscriber;
                    }
                }

                /// An aggregate event notified whenever any menu element in this class has its value changed.
                public event System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent>? OnValueChanged;

                public SubDataMenu()
                {
                    __MyString_subscriber = value => InvokeValueChanged(new(nameof(MyString), value));
                    MyString = new Silksong.ModMenu.Elements.TextInput<string>("My String", Silksong.ModMenu.Models.TextModels.ForStrings(), "");
                }

                /// <inheritdoc />
                public void ExportTo(Test.SubData data)
                {
                    data.MyString = MyString.Value;
                }

                /// <inheritdoc />
                public void ApplyFrom(Test.SubData data)
                {
                    using (notifySubscribers.Suppress())
                    {
                        MyString.Value = data.MyString;
                    }
                }

                /// <inheritdoc />
                public System.Collections.Generic.IEnumerable<Silksong.ModMenu.Elements.MenuElement> Elements()
                {
                    yield return MyString;
                }

                private readonly Silksong.ModMenu.Util.EventSuppressor notifySubscribers = new();

                private void InvokeValueChanged(Silksong.ModMenu.Generator.CustomMenuValueChangedEvent args)
                {
                    if (notifySubscribers.Suppressed) return;
                    OnValueChanged?.Invoke(args);
                }

                private readonly System.Action<string> __MyString_subscriber;
            }
            """;

        await ExpectSourceCode(source, ("TestDataMenu.g.cs", gen1), ("SubDataMenu.g.cs", gen2));
    }

    // TODO: Add Diagnostic tests.

    private static Task ExpectSourceCode(
        string source,
        params (string name, string text)[] generated
    )
    {
        GenTest test = new()
        {
            TestCode = source,
            TestState =
            {
                AdditionalReferences =
                {
                    typeof(GenerateMenuAttribute).Assembly,
                    typeof(ModMenuGenerator).Assembly,
                },
                ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            },
        };
        foreach (var (name, text) in generated)
            test.TestState.GeneratedSources.Add((typeof(ModMenuGenerator), name, text));

        return test.RunAsync(TestContext.Current.CancellationToken);
    }

    private class GenTest : CSharpSourceGeneratorTest<ModMenuGenerator, DefaultVerifier>
    {
        protected override IEnumerable<Type> GetSourceGenerators() => [typeof(ModMenuGenerator)];

        protected override ParseOptions CreateParseOptions() =>
            new CSharpParseOptions(LanguageVersion.Preview, DocumentationMode.Diagnose);
    }
}
