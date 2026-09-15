using System.Collections.Generic;
using GlobalEnums;
using InControl;
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
//  3) Vanilla only supports single keys. ModMenu also handles KeyboardShortcuts with modifiers,
//     so it has been split into KeyCodeMappableKey and KeyboardShortcutMappableKey.
internal abstract class CustomMappableKey
    : MenuButton,
        ISubmitHandler,
        IEventSystemHandler,
        IPointerClickHandler,
        ICancelHandler
{
    protected static readonly HashSet<Key> unmappableKeys = [Key.Escape, Key.Return, Key.Numlock];

    internal Text? KeymapText { get; private set; }
    internal Image? KeymapImage { get; private set; }

    internal static T Replace<T>(MappableKey src)
        where T : CustomMappableKey
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

        T dest;
        using (obj.TempInactive())
        {
            dest = obj.AddComponent<T>();
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
            dest.OnReplaced();
        }

        return dest;
    }

    private bool isListening;

    protected abstract void ResetListener();

    protected abstract void ListenUpdate();

    protected abstract Key CurrentMainKey { get; }

    protected abstract void ShowCurrentValue();

    protected virtual void ClearCustomVisuals() { }

    protected virtual void OnReplaced() { }

    private new void OnDisable()
    {
        if (isListening)
            AbortRebind();
        base.OnDisable();
    }

    private static UIButtonSkins UIButtonSkins => GameManager.instance.ui.uiButtonSkins;

    private void StartListening()
    {
        if (isListening || KeymapText == null || KeymapImage == null)
            return;

        interactable = false;
        isListening = true;
        ResetListener();
        ShowCurrentBinding();
    }

    protected void StopListening()
    {
        isListening = false;
        interactable = true;
    }

    private void Update()
    {
        if (!isListening)
            return;

        ListenUpdate();
    }

    public void ShowCurrentBinding()
    {
        if (KeymapText == null || KeymapImage == null)
            return;

        ClearCustomVisuals();

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
        else
            ShowCurrentValue();
    }

    /// <summary>
    /// The button skin for a key with a fallback for keys unbindable in vanilla.
    /// </summary>
    protected static ButtonSkin GetSkinFor(Key key)
    {
        var skins = UIButtonSkins;
        var skin = skins.GetButtonSkinFor(key.ToString());
        if (skin.skinType == ButtonSkinType.BLANK)
        {
            skin.sprite = skins.rectangleKey;
            skin.skinType = ButtonSkinType.WIDE;
            skin.symbol = key switch
            {
                Key.LeftCommand => "L Cmd",
                Key.RightCommand => "R Cmd",
                _ => skin.symbol,
            };
        }
        return skin;
    }

    /// <summary>
    /// The width a sprite visually occupies within a key cap RectTransform
    /// </summary>
    protected static float RenderedSpriteWidth(Sprite sprite, Vector2 slotSize) =>
        Mathf.Min(slotSize.x, sprite.rect.width / sprite.rect.height * slotSize.y);

    /// <summary>
    /// The horizontal offset that centers a key label on the keycap sprite.
    /// </summary>
    private static float CapLabelOffset(Image image)
    {
        var slot = image.rectTransform.sizeDelta;
        return (slot.x - RenderedSpriteWidth(image.sprite, slot)) / 2;
    }

    /// <summary>
    /// Apply the visual style for a single key cap from its button skin.
    /// </summary>
    protected void ApplyButtonSkin(Image image, Text text, ButtonSkin skin)
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

    internal void AbortRebind()
    {
        if (!isListening || KeymapText == null || KeymapImage == null)
            return;

        StopListening();
        ShowCurrentBinding();
    }

    public new void OnSubmit(BaseEventData eventData) => StartListening();

    public new void OnPointerClick(PointerEventData eventData) => StartListening();

    public new void OnCancel(BaseEventData eventData)
    {
        if (isListening)
            AbortRebind();
        else
            base.OnCancel(eventData);
    }
}
