using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal static class MenuButtonListUtil
{
    private static readonly Type entryType = typeof(MenuButtonList).GetNestedType(
        "Entry",
        BindingFlags.NonPublic
    );
    private static readonly ConstructorInfo entryConstructor = entryType.GetConstructor(
        Type.EmptyTypes
    );
    private static readonly FieldInfo entrySelectableField = entryType.GetField(
        "selectable",
        BindingFlags.NonPublic | BindingFlags.Instance
    );

    private static readonly FieldInfo entriesField = typeof(MenuButtonList).GetField(
        "entries",
        BindingFlags.NonPublic | BindingFlags.Instance
    );

    internal static void InsertButton(this MenuButtonList self, MenuButton button, int index)
    {
        Array array = (entriesField.GetValue(self) as Array)!;
        List<object> list = [.. array];

        object entry = entryConstructor.Invoke([]);
        entrySelectableField.SetValue(entry, button);
        list.Insert(index, entry);

        Array newArray = Array.CreateInstance(entryType, list.Count);
        Array.Copy(list.ToArray(), newArray, list.Count);
        entriesField.SetValue(self, newArray);
    }

    internal static void AppendButton(this MenuButtonList self, MenuButton button)
    {
        Array array = (entriesField.GetValue(self) as Array)!;
        self.InsertButton(button, array.Length);
    }
}
