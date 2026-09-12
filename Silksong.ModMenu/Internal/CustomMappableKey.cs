using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using GlobalEnums;
using InControl;
using Silksong.ModMenu.Models;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

// This is a partial transcription of MappableKey (though it has been dramatically changed).
// The source that follows is basically the original except:
//  1) All references to PlayerActions and BindingSources have been removed.
//  2) Methods that now do nothing have been removed / inlined.
//  3) Dumb things like member variables that should be constants have been refactored.
internal class CustomMappableKey
    : MenuButton,
        ISubmitHandler,
        IEventSystemHandler,
        IPointerClickHandler,
        ICancelHandler
{
    private static readonly HashSet<Key> unmappableKeys = [Key.Escape, Key.Return, Key.Numlock];

    /// <summary>
    /// The width of the '+' separators in key combination shortcuts.
    /// </summary>
    private const float ComboSeparatorWidth = 50f;

    /// <summary>
    /// The key cap and separator objects of a key combination shortcut.
    /// </summary>
    private readonly List<GameObject> comboCaps = [];

    internal Text? KeymapText { get; private set; }
    internal Image? KeymapImage { get; private set; }

    internal static CustomMappableKey Replace(MappableKey src)
    {
        // We copy all the necessary fields over one-by-one rather than modifying the MappableKey source code directly.
        // The number and scale of source edits required to decouple from PlayerAction would be far worse to execute via ILHooks.
        var animationTriggers = src.animationTriggers;
        var colors = src.colors;
        var keymapImage = src.keymapSprite;
        var keymapText = src.keymapText;
        var leftCursor = src.leftCursor;
        var menuCancelVibration = src.menuCancelVibration;
        var menuSubmitVibration = src.menuSubmitVibration;
        var rightCursor = src.rightCursor;

        var obj = src.gameObject;
        DestroyImmediate(src); // We cannot wait 1 frame to add a new Selectable component.

        CustomMappableKey dest;
        using (obj.TempInactive())
        {
            dest = obj.AddComponent<CustomMappableKey>();
            dest.animationTriggers = animationTriggers;
            dest.buttonType = MenuButtonType.Proceed;
            dest.cancelAction = CancelAction.DoNothing;
            dest.colors = colors;
            dest.DontPlaySelectSound = true;
            dest.KeymapImage = keymapImage;
            dest.KeymapText = keymapText;
            dest.leftCursor = leftCursor;
            dest.menuCancelVibration = menuCancelVibration;
            dest.menuSubmitVibration = menuSubmitVibration;
            dest.playSubmitSound = true;
            dest.prevSelectedObject = obj;
            dest.rightCursor = rightCursor;
            dest.transition = Transition.None;
            dest.uiAudioPlayer = UIManager.instance.uiAudioPlayer;
            dest.originalKeymapSize = keymapImage.rectTransform.sizeDelta;
        }

        return dest;
    }

    private bool isListening;
    private readonly KeyBindingSourceListener keyCodeListener = new();
    private readonly KeyShortcutSourceListener shortcutListener = new();

    private Vector2 originalKeymapSize;

    internal IValueModel<KeyCode>? KeyCodeModel
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field?.OnValueChanged -= OnKeyCodeChanged;
            field = value;
            field?.OnValueChanged += OnKeyCodeChanged;

            ShowCurrentKeyCode();
        }
    }

    private void OnKeyCodeChanged(KeyCode keyCode) => ShowCurrentKeyCode();

    /// <summary>
    /// The value model exposing the full shortcut, including modifiers.
    /// Only when this is set can modifiers be recorded.
    /// </summary>
    internal IValueModel<KeyboardShortcut>? ShortcutModel
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field?.OnValueChanged -= OnShortcutChanged;
            field = value;
            field?.OnValueChanged += OnShortcutChanged;

            ShowCurrentKeyCode();
        }
    }

    private void OnShortcutChanged(KeyboardShortcut shortcut) => ShowCurrentKeyCode();

    private KeyboardShortcut CurrentShortcut =>
        ShortcutModel?.Value ?? new KeyboardShortcut(KeyCodeModel?.Value ?? KeyCode.None);

    private Key CurrentMainKey =>
        isListening ? Key.None : KeyCodeUtil.ToKey(CurrentShortcut.MainKey);

    private new void OnDisable()
    {
        if (isListening)
            AbortRebind();
        base.OnDisable();
    }

    private static UIButtonSkins UIButtonSkins => GameManager.instance.ui.uiButtonSkins;

    private void ListenForNewButton()
    {
        if (isListening || KeymapText == null || KeymapImage == null)
            return;

        interactable = false;
        isListening = true;
        keyCodeListener.Reset();
        shortcutListener.Reset();
        ShowCurrentKeyCode();
    }

    private void Update()
    {
        if (!isListening)
            return;

        if (ShortcutModel != null)
            ListenForShortcut();
        else
            ListenForKeyCode();
    }

    /// <summary>
    /// Rebind flow for single key codes
    /// </summary>
    private void ListenForKeyCode()
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
            isListening = false;
            interactable = true;
            KeyCodeModel?.Value = KeyCodeUtil.ToKeyCode(key);
            ShowCurrentKeyCode();
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

    /// <summary>
    /// Rebind flow for full shortcuts
    /// </summary>
    private void ListenForShortcut()
    {
        var recording = shortcutListener.Listen();
        if (recording is not { } recorded)
            return;

        if (unmappableKeys.Contains(recorded.MainKey))
        {
            AbortRebind();
            return;
        }

        AcceptShortcut(
            new KeyboardShortcut(
                KeyCodeUtil.ToKeyCode(recorded.MainKey),
                [.. recorded.Modifiers.Select(KeyCodeUtil.ToKeyCode)]
            )
        );
    }

    private void AcceptShortcut(KeyboardShortcut shortcut)
    {
        isListening = false;
        interactable = true;
        ShortcutModel?.Value = shortcut;
        ShowCurrentKeyCode();
    }

    public void ShowCurrentKeyCode()
    {
        if (KeymapText == null || KeymapImage == null)
            return;

        ClearComboCaps();

        // Resize to vanilla if the main key was shrunk to be flush with the keys.
        KeymapImage.rectTransform.sizeDelta = originalKeymapSize;

        var skins = UIButtonSkins;
        if (isListening)
        {
            KeymapImage.sprite = UIButtonSkins.blankKey;
            KeymapText.text = Language.Get("KEYBOARD_PRESSKEY", "MainMenu");
            KeymapText.fontSize = MappableKey.blankFontSize;
            KeymapText.alignment = MappableKey.blankAlignment;
            KeymapText.horizontalOverflow = MappableKey.blankOverflow;
            KeymapText.GetComponent<FixVerticalAlign>().AlignText();
        }
        else if (CurrentMainKey == Key.None)
        {
            KeymapImage.sprite = skins.blankKey;
            KeymapText.text = Language.Get("KEYBOARD_UNMAPPED", "MainMenu");
            KeymapText.fontSize = MappableKey.blankFontSize;
            KeymapText.alignment = MappableKey.blankAlignment;
            KeymapText.resizeTextForBestFit = MappableKey.blankBestFit;
            KeymapText.horizontalOverflow = MappableKey.blankOverflow;
            KeymapText.GetComponent<FixVerticalAlign>().AlignText();
        }
        else if (CurrentShortcut.Modifiers.Any())
        {
            ShowCombo(CurrentShortcut);
        }
        else
        {
            ApplyButtonSkin(
                KeymapImage,
                KeymapText,
                skins.GetButtonSkinFor(CurrentMainKey.ToString())
            );
        }
    }

    private void ShowCombo(KeyboardShortcut shortcut)
    {
        var skins = UIButtonSkins;
        var keymapRT = KeymapImage!.rectTransform;

        var mainSkin = skins.GetButtonSkinFor(CurrentMainKey.ToString());
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
                NewComboCap(skins.GetButtonSkinFor(orderedModifiers[i].ToString())),
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

    /// <summary>
    /// The width a sprite visually occupies within a key cap RectTransform
    /// </summary>
    private static float RenderedSpriteWidth(Sprite sprite, Vector2 slotSize) =>
        Mathf.Min(slotSize.x, sprite.rect.width / sprite.rect.height * slotSize.y);

    /// <summary>
    /// The horizontal offset that centers a key label on the keycap sprite.
    /// </summary>
    private static float CapLabelOffset(Image image)
    {
        var slot = image.rectTransform.sizeDelta;
        return (slot.x - RenderedSpriteWidth(image.sprite, slot)) / 2;
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

    private void ClearComboCaps()
    {
        foreach (var cap in comboCaps)
            DestroyImmediate(cap);

        comboCaps.Clear();
    }

    /// <summary>
    /// Apply the visual style for a single key cap from its button skin.
    /// </summary>
    private void ApplyButtonSkin(Image image, Text text, ButtonSkin skin)
    {
        var skins = UIButtonSkins;
        image.sprite = skin.sprite != null ? skin.sprite : skins.blankKey;
        text.text = skin.symbol;
        if (skin.skinType == ButtonSkinType.SQUARE)
        {
            text.fontSize = MappableKey.sqrFontSize;
            text.alignment = MappableKey.sqrAlignment;
            text.rectTransform.anchoredPosition = new(
                CapLabelOffset(image),
                text.rectTransform.anchoredPosition.y
            );
            text.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                MappableKey.sqrWidth
            );
            text.resizeTextForBestFit = MappableKey.sqrBestFit;
            text.resizeTextMinSize = MappableKey.sqrMinFont;
            text.resizeTextMaxSize = MappableKey.sqrMaxFont;
            text.horizontalOverflow = MappableKey.sqrHOverflow;
        }
        else if (skin.skinType == ButtonSkinType.WIDE)
        {
            text.fontSize = MappableKey.wideFontSize;
            text.alignment = MappableKey.wideAlignment;
            text.rectTransform.anchoredPosition = new(
                CapLabelOffset(image),
                text.rectTransform.anchoredPosition.y
            );
            text.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                MappableKey.wideWidth
            );
            text.resizeTextForBestFit = MappableKey.wideBestFit;
            text.horizontalOverflow = MappableKey.wideHOverflow;
        }
        else
            text.alignment = skins.labelAlignment;

        text.GetComponent<FixVerticalAlign>().AlignTextKeymap();
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

    internal void AbortRebind()
    {
        if (!isListening || KeymapText == null || KeymapImage == null)
            return;

        interactable = true;
        isListening = false;
        ShowCurrentKeyCode();
    }

    public new void OnSubmit(BaseEventData eventData) => ListenForNewButton();

    public new void OnPointerClick(PointerEventData eventData) => ListenForNewButton();

    public new void OnCancel(BaseEventData eventData)
    {
        if (isListening)
            AbortRebind();
        else
            base.OnCancel(eventData);
    }
}
