using System.Collections.Generic;
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
    private static readonly HashSet<KeyBindingSource> unmappableKeys =
    [
        new(Key.Escape),
        new(Key.Return),
        new(Key.Numlock),
        new(Key.LeftCommand),
        new(Key.RightCommand),
    ];

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

        var wasActive = obj.activeSelf;
        obj.SetActive(false);
        CustomMappableKey dest = obj.AddComponent<CustomMappableKey>();
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
        obj.SetActive(wasActive);

        return dest;
    }

    private bool isListening;
    private readonly KeyBindingSourceListener listener = new();

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

    private InputHandler.KeyOrMouseBinding CurrentBinding =>
        new(isListening ? Key.None : KeyCodeUtil.ToKey(KeyCodeModel?.Value ?? KeyCode.None));

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
        listener.Reset();
        ShowCurrentKeyCode();
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

    private void Update()
    {
        if (!isListening)
            return;

        var source = listener.Listen(
            new() { IncludeKeys = true, IncludeModifiersAsFirstClassKeys = true },
            InputManager.ActiveDevice
        );

        if (
            source is KeyBindingSource keyBinding
            && !unmappableKeys.Contains(keyBinding)
            && GetKey(keyBinding, out var key)
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

    public void ShowCurrentKeyCode()
    {
        if (KeymapText == null || KeymapImage == null)
            return;

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
        else if (InputHandler.KeyOrMouseBinding.IsNone(CurrentBinding))
        {
            KeymapImage.sprite = skins.blankKey;
            KeymapText.text = Language.Get("KEYBOARD_UNMAPPED", "MainMenu");
            KeymapText.fontSize = MappableKey.blankFontSize;
            KeymapText.alignment = MappableKey.blankAlignment;
            KeymapText.resizeTextForBestFit = MappableKey.blankBestFit;
            KeymapText.horizontalOverflow = MappableKey.blankOverflow;
            KeymapText.GetComponent<FixVerticalAlign>().AlignText();
        }
        else
        {
            ButtonSkin keyboardSkinFor = skins.GetButtonSkinFor(CurrentBinding.ToString());
            KeymapImage.sprite =
                keyboardSkinFor.sprite != null ? keyboardSkinFor.sprite : skins.blankKey;
            KeymapText.text = keyboardSkinFor.symbol;
            if (keyboardSkinFor.skinType == ButtonSkinType.SQUARE)
            {
                KeymapText.fontSize = MappableKey.sqrFontSize;
                KeymapText.alignment = MappableKey.sqrAlignment;
                KeymapText.rectTransform.anchoredPosition = new(
                    MappableKey.sqrX,
                    KeymapText.rectTransform.anchoredPosition.y
                );
                KeymapText.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    MappableKey.sqrWidth
                );
                KeymapText.resizeTextForBestFit = MappableKey.sqrBestFit;
                KeymapText.resizeTextMinSize = MappableKey.sqrMinFont;
                KeymapText.resizeTextMaxSize = MappableKey.sqrMaxFont;
                KeymapText.horizontalOverflow = MappableKey.sqrHOverflow;
            }
            else if (keyboardSkinFor.skinType == ButtonSkinType.WIDE)
            {
                KeymapText.fontSize = MappableKey.wideFontSize;
                KeymapText.alignment = MappableKey.wideAlignment;
                KeymapText.rectTransform.anchoredPosition = new(
                    MappableKey.wideX,
                    KeymapText.rectTransform.anchoredPosition.y
                );
                KeymapText.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    MappableKey.wideWidth
                );
                KeymapText.resizeTextForBestFit = MappableKey.wideBestFit;
                KeymapText.horizontalOverflow = MappableKey.wideHOverflow;
            }
            else
                KeymapText.alignment = skins.labelAlignment;

            KeymapText.GetComponent<FixVerticalAlign>().AlignTextKeymap();
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
