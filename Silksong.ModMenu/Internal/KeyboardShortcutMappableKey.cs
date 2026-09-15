using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using InControl;
using Silksong.ModMenu.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// Custom Mappable Key capable of displaying and capturing keyboard shortcuts with multiple modifiers.
/// </summary>
internal class KeyboardShortcutMappableKey : CustomMappableKey
{
    /// <summary>
    /// The width of the '+' separators in key combination shortcuts.
    /// </summary>
    private const float ComboSeparatorWidth = 50f;

    /// <summary>
    /// The key cap and separator objects of a key combination shortcut.
    /// </summary>
    private readonly List<GameObject> comboCaps = [];

    private Vector2 originalKeymapSize;
    private float originalKeymapTextX;
    private float originalKeymapTextWidth;

    private readonly KeyShortcutSourceListener shortcutListener = new();

    /// <summary>
    /// The value model exposing the full shortcut, including modifiers.
    /// Only when this is set can modifiers be recorded.
    /// </summary>
    internal IValueModel<KeyboardShortcut>? Model
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field?.OnValueChanged -= OnShortcutChanged;
            field = value;
            field?.OnValueChanged += OnShortcutChanged;

            ShowCurrentBinding();
        }
    }

    private void OnShortcutChanged(KeyboardShortcut shortcut) => ShowCurrentBinding();

    private KeyboardShortcut CurrentShortcut => Model?.Value ?? KeyboardShortcut.Empty;

    protected override Key CurrentMainKey => KeyCodeUtil.ToKey(CurrentShortcut.MainKey);

    protected override void ResetListener() => shortcutListener.Reset();

    protected override void OnReplaced()
    {
        originalKeymapSize = KeymapImage!.rectTransform.sizeDelta;
        originalKeymapTextX = KeymapText!.rectTransform.anchoredPosition.x;
        originalKeymapTextWidth = KeymapText!.rectTransform.sizeDelta.x;
    }

    /// <summary>
    /// Rebind flow for full shortcuts
    /// </summary>
    protected override void ListenUpdate()
    {
        var recording = shortcutListener.Listen();
        if (recording is not { } recorded)
            return;

        if (unmappableKeys.Contains(recorded.MainKey))
        {
            AbortRebind();
            return;
        }

        StopListening();
        Model?.Value = new KeyboardShortcut(
            KeyCodeUtil.ToKeyCode(recorded.MainKey),
            [.. recorded.Modifiers.Select(KeyCodeUtil.ToKeyCode)]
        );
        ShowCurrentBinding();
    }

    protected override void ShowCurrentValue()
    {
        if (CurrentShortcut.Modifiers.Any())
            ShowCombo(CurrentShortcut);
        else
            ApplyButtonSkin(KeymapImage!, KeymapText!, GetSkinFor(CurrentMainKey));
    }

    protected override void ClearCustomVisuals()
    {
        foreach (var cap in comboCaps)
            DestroyImmediate(cap);

        comboCaps.Clear();

        // Resize to vanilla if the main key was shrunk to be flush with the keys.
        KeymapImage!.rectTransform.sizeDelta = originalKeymapSize;
        var textRect = KeymapText!.rectTransform;
        textRect.anchoredPosition = textRect.anchoredPosition with { x = originalKeymapTextX };
        textRect.sizeDelta = textRect.sizeDelta with { x = originalKeymapTextWidth };
    }

    private void ShowCombo(KeyboardShortcut shortcut)
    {
        var keymapRT = KeymapImage!.rectTransform;

        var mainSkin = GetSkinFor(CurrentMainKey);
        // Size the main key to be only as wide as the sprite.
        float mainWidth = RenderedSpriteWidth(mainSkin.sprite, originalKeymapSize);
        keymapRT.sizeDelta = originalKeymapSize with { x = mainWidth };
        ApplyButtonSkin(KeymapImage, KeymapText!, mainSkin);

        float occupied = mainWidth;
        var orderedModifiers = DisplayOrderedModifiers(shortcut);
        for (int i = orderedModifiers.Length - 1; i >= 0; i--)
        {
            PlaceComboElement(NewComboSeparator(), ref occupied);
            PlaceComboElement(
                NewComboCap(GetSkinFor(KeyCodeUtil.ToKey(orderedModifiers[i]))),
                ref occupied
            );
        }
    }

    /// <summary>
    /// The shortcut's modifiers in conventional display order
    /// </summary>
    private static KeyCode[] DisplayOrderedModifiers(KeyboardShortcut shortcut) =>
        [.. shortcut.Modifiers.OrderBy(ModifierDisplayOrder).ThenByDescending(m => m)];

    private static int ModifierDisplayOrder(KeyCode modifier) =>
        modifier switch
        {
            KeyCode.LeftControl or KeyCode.RightControl => 0,
            KeyCode.LeftShift or KeyCode.RightShift => 1,
            KeyCode.LeftAlt or KeyCode.RightAlt => 2,
            KeyCode.LeftCommand or KeyCode.RightCommand => 3,
            _ => int.MaxValue,
        };

    private GameObject NewComboCap(ButtonSkin skin)
    {
        var cap = Instantiate(KeymapImage!.gameObject, transform);
        cap.name = "Keymap Modifier";
        comboCaps.Add(cap);
        cap.GetComponent<RectTransform>().sizeDelta = originalKeymapSize with
        {
            x = RenderedSpriteWidth(skin.sprite, originalKeymapSize),
        };
        ApplyButtonSkin(cap.GetComponent<Image>(), cap.GetComponentInChildren<Text>(), skin);
        return cap;
    }

    private GameObject NewComboSeparator()
    {
        var separator = new GameObject("Keymap Separator", typeof(Text));
        var rect = separator.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        var keymapRect = KeymapImage!.rectTransform;
        rect.anchorMin = keymapRect.anchorMin;
        rect.anchorMax = keymapRect.anchorMax;
        rect.pivot = keymapRect.pivot;
        rect.anchoredPosition = keymapRect.anchoredPosition;
        rect.sizeDelta = originalKeymapSize with { x = ComboSeparatorWidth };

        var text = separator.GetComponent<Text>();
        text.font = KeymapText!.font;
        text.color = KeymapText.color;
        text.raycastTarget = KeymapText.raycastTarget;
        text.text = "+";
        text.fontSize = MappableKey.wideFontSize;
        text.alignment = TextAnchor.MiddleCenter;
        separator.AddComponent<FixVerticalAlign>().AlignTextKeymap();

        comboCaps.Add(separator);
        return separator;
    }

    private static void PlaceComboElement(GameObject element, ref float occupied)
    {
        var rectTransform = element.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = rectTransform.anchoredPosition with
        {
            x = rectTransform.anchoredPosition.x - occupied,
        };
        occupied += rectTransform.sizeDelta.x;
    }

    internal void SetKeymapColor(Color color)
    {
        KeymapImage!.color = color;
        KeymapText!.color = color;

        foreach (var cap in comboCaps)
        {
            if (cap.TryGetComponent<Image>(out var image))
                image.color = color;
            cap.GetComponentInChildren<Text>().color = color;
        }
    }
}
