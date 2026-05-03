using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Silksong.ModMenu.Generator;
using Silksong.ModMenuAnalyzers;

namespace Silksong.ModMenuAnalyzerTest;

public class GeneratorTest
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
                    get => _MyInt;
                    set
                    {
                        if (value == null) throw new System.ArgumentNullException(nameof(MyInt));
                        if (_MyInt == value) return;

                        if (_MyInt != null)
                            _MyInt.OnValueChanged -= _MyInt_subscriber;
                        _MyInt = value;
                        _MyInt.OnValueChanged += _MyInt_subscriber;
                    }
                }
                private Silksong.ModMenu.Elements.SelectableValueElement<int> _MyInt;

                /// An aggregate event notified whenever any menu element in this class has its value changed.
                public event System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent>? OnValueChanged;

                public TestDataMenu()
                {
                    _MyInt_subscriber = value => InvokeValueChanged(new(nameof(MyInt), value));
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

                private readonly System.Action<int> _MyInt_subscriber;
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
                    get => _SubData;
                    set
                    {
                        if (value == null) throw new System.ArgumentNullException(nameof(SubData));
                        if (_SubData == value) return;

                        if (_SubData != null)
                            _SubData.SubMenu.OnValueChanged -= _SubData_subscriber;
                        _SubData = value;
                        _SubData.SubMenu.OnValueChanged += _SubData_subscriber;
                    }
                }
                private Silksong.ModMenu.Generator.SubMenuElement<Test.SubData, SubDataMenu> _SubData;

                /// An aggregate event notified whenever any menu element in this class has its value changed.
                public event System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent>? OnValueChanged;

                public TestDataMenu()
                {
                    _SubData_subscriber = _ => InvokeValueChanged(new(nameof(SubData), null));
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

                private readonly System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent> _SubData_subscriber;
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
                    get => _MyString;
                    set
                    {
                        if (value == null) throw new System.ArgumentNullException(nameof(MyString));
                        if (_MyString == value) return;

                        if (_MyString != null)
                            _MyString.OnValueChanged -= _MyString_subscriber;
                        _MyString = value;
                        _MyString.OnValueChanged += _MyString_subscriber;
                    }
                }
                private Silksong.ModMenu.Elements.SelectableValueElement<string> _MyString;

                /// An aggregate event notified whenever any menu element in this class has its value changed.
                public event System.Action<Silksong.ModMenu.Generator.CustomMenuValueChangedEvent>? OnValueChanged;

                public SubDataMenu()
                {
                    _MyString_subscriber = value => InvokeValueChanged(new(nameof(MyString), value));
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

                private readonly System.Action<string> _MyString_subscriber;
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
    }
}
