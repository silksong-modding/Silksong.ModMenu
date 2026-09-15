using System.Collections.Generic;
using InControl;
using Silksong.ModMenu.Models;
using UnityEngine;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Custom Mappable Key of displaying and capturing single keys.
/// </summary>
internal class KeyCodeMappableKey : CustomMappableKey
{
    private readonly KeyBindingSourceListener keyCodeListener = new();

    internal IValueModel<KeyCode>? Model
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field?.OnValueChanged -= OnKeyCodeChanged;
            field = value;
            field?.OnValueChanged += OnKeyCodeChanged;

            ShowCurrentBinding();
        }
    }

    private void OnKeyCodeChanged(KeyCode keyCode) => ShowCurrentBinding();

    protected override Key CurrentMainKey => KeyCodeUtil.ToKey(Model?.Value ?? KeyCode.None);

    protected override void ResetListener() => keyCodeListener.Reset();

    /// <summary>
    /// Rebind flow for single key codes
    /// </summary>
    protected override void ListenUpdate()
    {
        var source = keyCodeListener.Listen(
            new() { IncludeKeys = true, IncludeModifiersAsFirstClassKeys = true },
            InputManager.ActiveDevice
        );

        if (
            source is KeyBindingSource keyBinding
            && GetKey(keyBinding, out var key)
            && !unmappableKeys.Contains(key)
        )
        {
            StopListening();
            Model?.Value = KeyCodeUtil.ToKeyCode(key);
            ShowCurrentBinding();
        }
        else if (source != null)
            AbortRebind();
    }

    private static bool GetKey(KeyBindingSource keyBinding, out Key key)
    {
        List<Key> keys = [];
        for (int i = 0; i < keyBinding.Control.IncludeCount; i++)
        {
            var ret = keyBinding.Control.GetInclude(i);
            if (ret != Key.None)
                keys.Add(ret);
        }

        if (keys.Count == 1)
        {
            key = keys[0];
            return true;
        }
        else
        {
            key = Key.None;
            return false;
        }
    }

    protected override void ShowCurrentValue() =>
        ApplyButtonSkin(KeymapImage!, KeymapText!, GetSkinFor(CurrentMainKey));
}
