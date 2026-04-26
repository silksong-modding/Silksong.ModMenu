using GlobalEnums;
using MonoDetour;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using Silksong.ModMenu.Elements;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

[MonoDetourTargets(typeof(MappableKey), GenerateControlFlowVariants = true)]
internal class MenuPrefabs
{
    private static MenuPrefabs? instance;

    internal static MenuPrefabs Get() =>
        instance ?? throw new System.Exception("UIManager not initialized yet");

    internal static void Load(UIManager uiManager) => instance ??= new(uiManager);

    private readonly GameObject canvas;

    private readonly GameObject menuTemplate;
    private readonly GameObject keyBindTemplate;
    private readonly GameObject textButtonTemplate;
    private readonly GameObject textLabelTemplate;
    private readonly GameObject textChoiceTemplate;
    private readonly GameObject textInputTemplate;
    private readonly GameObject sliderTemplate;
    private readonly GameObject scrollPaneTemplate;

    private MenuPrefabs(UIManager uiManager)
    {
        canvas = uiManager.gameObject.FindChild("UICanvas")!;
        var optionsScreen = canvas.FindChild("OptionsMenuScreen")!;

        // On destruction, reset MenuPrefabs.
        uiManager.gameObject.GetOrAddComponent<OnDestroyHelper>().Action += () =>
        {
            if (instance == this)
                instance = null;
        };

        menuTemplate = Object.Instantiate(optionsScreen);
        menuTemplate.SetActive(false);
        menuTemplate.name = "ModMenuScreen";
        menuTemplate.RemoveComponent<MenuButtonList>();
        menuTemplate.FindChild("Title")!.RemoveComponent<AutoLocalizeTextUI>();
        Object.Destroy(menuTemplate.FindChild("Content")!);
        Object.DontDestroyOnLoad(menuTemplate);

        // MappableKey.OnEnable() breaks when instantiated outside the UIButtonSkins hierarchy.
        using (mappableKeyInit.Suppress())
        {
            keyBindTemplate = Object.Instantiate(
                canvas.FindChild("KeyboardMenuScreen/Content/MappableKeys/UpButton")!
            );
        }
        keyBindTemplate.SetActive(false);
        keyBindTemplate
            .FindChild("Input Button Text")!
            .RemoveComponent<ChangeTextFontScaleOnHandHeld>();
        Object.DontDestroyOnLoad(keyBindTemplate);

        textLabelTemplate = Object.Instantiate(
            optionsScreen.FindChild("Content/GameOptions/GameOptionsButton/Menu Button Text")!
        );
        textLabelTemplate.SetActive(false);
        textLabelTemplate.RemoveComponent<ChangeTextFontScaleOnHandHeld>();
        textLabelTemplate.name = "TextLabel";
        textLabelTemplate.GetComponent<Text>().raycastTarget = false;
        Object.DontDestroyOnLoad(textLabelTemplate);

        textChoiceTemplate = Object.Instantiate(
            canvas.FindChild("GameOptionsMenuScreen/Content/CamShakeSetting")!
        );
        textChoiceTemplate.SetActive(false);
        textChoiceTemplate.name = "ValueChoiceContainer";
        Object.DontDestroyOnLoad(textChoiceTemplate);

        var choiceChild = textChoiceTemplate.FindChild("CamShakePopupOption")!;
        choiceChild.name = "ValueChoice";
        choiceChild.RemoveComponent<MenuSetting>();
        var moh = choiceChild.GetComponent<MenuOptionHorizontal>();
        moh.optionList = ["###INTERNAL###"];
        moh.menuSetting = null;
        moh.localizeText = false;
        moh.applyButton = null;
        choiceChild
            .FindChild("Menu Option Label")!
            .RemoveComponent<ChangeTextFontScaleOnHandHeld>();
        choiceChild.FindChild("Menu Option Text")!.RemoveComponent<ChangeTextFontScaleOnHandHeld>();
        choiceChild.FindChild("Description")!.RemoveComponent<ChangeTextFontScaleOnHandHeld>();

        textButtonTemplate = Object.Instantiate(optionsScreen.FindChild("Content/GameOptions")!);
        textButtonTemplate.SetActive(false);
        textButtonTemplate.name = "TextButtonContainer";
        Object.DontDestroyOnLoad(textButtonTemplate);

        var buttonChild = textButtonTemplate.FindChild("GameOptionsButton")!;
        buttonChild.name = "TextButton";
        // We have to remove this component as it's on a different GameObject to the Text,
        // so it won't be removed by the LocalizedTextExtensions
        buttonChild.RemoveComponent<AutoLocalizeTextUI>();
        buttonChild.FindChild("Menu Button Text")!.RemoveComponent<ChangeTextFontScaleOnHandHeld>();

        // Add a (centered) description to the menu button
        GameObject clonedDescription = Object.Instantiate(choiceChild.FindChild("Description")!);
        RectTransform clonedDescTransform = clonedDescription.GetComponent<RectTransform>();
        clonedDescTransform.SetParent(buttonChild.transform, worldPositionStays: false);
        clonedDescTransform.anchorMin = new Vector2(0.5f, 0.5f);
        clonedDescTransform.anchorMax = new Vector2(0.5f, 0.5f);
        clonedDescTransform.pivot = new Vector2(0.5f, 0.5f);
        clonedDescription.name = "Description";
        Text clonedDescText = clonedDescription.GetComponent<Text>();
        clonedDescText.alignment = TextAnchor.MiddleCenter;
        buttonChild.GetComponent<MenuButton>().descriptionText =
            clonedDescText.GetComponent<Animator>();

        textInputTemplate = Object.Instantiate(textChoiceTemplate);
        textInputTemplate.SetActive(false);
        textInputTemplate.name = "TextInputContainer";
        Object.DontDestroyOnLoad(textInputTemplate);

        var textInputChild = textInputTemplate.FindChild("ValueChoice")!;
        textInputChild.name = "TextInput";
        textInputChild.RemoveComponent<EventTrigger>();
        textInputChild.RemoveComponent<FixVerticalAlign>();
        Object.DestroyImmediate(textInputChild.GetComponent<MenuOptionHorizontal>()); // We must delete the Selectable immediately to add a new one.
        textInputChild.RemoveComponent<MenuSetting>();
        var textInputField = textInputChild.AddComponent<CustomInputField>();
        textInputField.textComponent = textInputChild
            .FindChild("Menu Option Text")!
            .GetComponent<Text>();
        textInputField.caretColor = Color.white;
        textInputField.contentType = InputField.ContentType.Standard;
        textInputField.caretWidth = 8;
        textInputField.text = "";
        textInputChild.AddComponent<MenuSelectableAnimationProxy>().Animators =
        [
            textInputChild.FindChild("Description")!.GetComponent<Animator>(),
            textInputChild.FindChild("CursorLeft")!.GetComponent<Animator>(),
            textInputChild.FindChild("CursorRight")!.GetComponent<Animator>(),
        ];

        sliderTemplate = Object.Instantiate(
            canvas.FindChild("AudioMenuScreen/Content/MasterVolume")!
        );
        sliderTemplate.SetActive(false);
        sliderTemplate.name = "SliderContainer";
        Object.DontDestroyOnLoad(sliderTemplate);

        var sliderChild = sliderTemplate.FindChild("MasterSlider")!;
        sliderChild.name = "Slider";
        sliderChild.GetComponent<Slider>().onValueChanged = new();
        sliderChild.RemoveComponent<MenuAudioSlider>();
        sliderChild.GetOrAddComponent<SliderRightStickInput>();
        sliderChild
            .FindChild("Menu Option Label")!
            .RemoveComponent<ChangeTextFontScaleOnHandHeld>();
        sliderChild.FindChild("MasterVolValue")!.name = "Value";

        scrollPaneTemplate = ConstructScrollPanePrefab(uiManager);
    }

    internal GameObject NewCustomMenu(LocalizedText title)
    {
        var obj = Object.Instantiate(menuTemplate);
        obj.name = $"ModMenuScreen-{title.Canonical}";
        obj.transform.SetParent(canvas.transform, false);
        obj.transform.localPosition = new(0, 10, 0);

        return obj;
    }

    internal GameObject NewKeyBindContainer(out CustomMappableKey customMappableKey)
    {
        var obj = Object.Instantiate(keyBindTemplate);
        customMappableKey = CustomMappableKey.Replace(obj.GetComponent<MappableKey>());
        return obj;
    }

    internal GameObject NewTextButtonContainer(out MenuButton menuButton)
    {
        var obj = Object.Instantiate(textButtonTemplate);
        menuButton = obj.FindChild("TextButton")!.GetComponent<MenuButton>();
        return obj;
    }

    internal GameObject NewTextLabel() => Object.Instantiate(textLabelTemplate);

    internal GameObject NewTextChoiceContainer(out MenuOptionHorizontal menuOptionHorizontal)
    {
        var obj = Object.Instantiate(textChoiceTemplate);
        menuOptionHorizontal = obj.FindChild("ValueChoice")!.GetComponent<MenuOptionHorizontal>();
        return obj;
    }

    internal GameObject NewTextInputContainer(out CustomInputField customInputField)
    {
        var obj = Object.Instantiate(textInputTemplate);
        customInputField = obj.FindChild("TextInput")!.GetComponent<CustomInputField>();
        return obj;
    }

    internal GameObject NewSliderContainer(out Slider slider)
    {
        var obj = Object.Instantiate(sliderTemplate);
        slider = obj.FindChild("Slider")!.GetComponent<Slider>();
        return obj;
    }

    internal GameObject NewScrollPane(
        out ScrollRect scrollRect,
        out ScrollFocusController focusController,
        out GameObject contentPane
    )
    {
        var obj = Object.Instantiate(scrollPaneTemplate);
        scrollRect = obj.GetComponent<ScrollRect>();
        focusController = obj.GetComponent<ScrollFocusController>();
        contentPane = obj.FindChild("Viewport/Content")!;
        return obj;
    }

    private static GameObject ConstructScrollPanePrefab(UIManager uiManager)
    {
        // Can't directly clone the achievements menu, it throws a bunch of errors,
        // but we can repurpose parts of its scrollbar.

        // Pulled this into its own function because it was getting quite long.

        #region Content Panes

        var scrollPane = new GameObject("ScrollPane") { layer = (int)PhysLayers.UI };
        scrollPane.SetActive(false);
        Object.DontDestroyOnLoad(scrollPane);
        scrollPane.AddComponent<Image>().color = Color.clear; // ensures it has a RectTransform
        var scrollPaneRT = (RectTransform)scrollPane.transform;
        scrollPaneRT.sizeDelta = new Vector2(
            1480,
            Mathf.Ceil(SpacingConstants.VSPACE_MEDIUM * 8.334f)
        );
        scrollPaneRT.pivot = new Vector2(0.5f, 1);

        var viewport = new GameObject("Viewport") { layer = (int)PhysLayers.UI };
        viewport.transform.SetParentReset(scrollPane.transform);
        viewport.AddComponent<Image>().color = Color.clear; // necessary for the RectMask2D
        viewport.AddComponent<RectMask2D>();
        var viewportRT = (RectTransform)viewport.transform;
        viewportRT.FitToParent();

        var content = new GameObject("Content") { layer = (int)PhysLayers.UI };
        content.transform.SetParentReset(viewport.transform);
        content.AddComponent<Image>().color = Color.clear; // ensures it has a RectTransform
        var contentRT = (RectTransform)content.transform;
        contentRT.anchorMax = contentRT.anchorMin = contentRT.pivot = new Vector2(0.5f, 1);

        #endregion
        #region Vertical "Scrollbar"

        var vScrollbar = Object.Instantiate(
            uiManager.achievementsMenuScreen.gameObject.FindChild("Content/Scrollbar")!,
            scrollPaneRT,
            false
        );
        vScrollbar.name = "Scrollbar V";

        ((RectTransform)vScrollbar.transform.Find("Background")).FitToParentVertical();

        var vScrollbarRT = (RectTransform)vScrollbar.transform;
        var slideAreaRT = (RectTransform)vScrollbarRT.Find("Sliding Area");
        var handleRT = (RectTransform)slideAreaRT.Find("Handle/TopFleur");

        slideAreaRT.FitToParent();
        slideAreaRT.sizeDelta = new Vector2(0, -150);

        Object.DestroyImmediate(handleRT.GetComponent<ScrollBarHandle>());
        handleRT.SetParentReset(slideAreaRT);
        handleRT.FitToParentHorizontal();
        handleRT.sizeDelta = new Vector2(0, 140);
        handleRT.pivot = Vector2.one * 0.5f;
        Object.DestroyImmediate(slideAreaRT.Find("Handle").gameObject);
        handleRT.gameObject.name = "Handle";

        vScrollbarRT.sizeDelta = new Vector2(50, 0);
        vScrollbarRT.FitToParentVertical(anchorX: 1);
        vScrollbarRT.anchoredPosition3D = vScrollbarRT.anchoredPosition3D with { z = -1 };
        vScrollbarRT.pivot = new Vector2(0, 0.5f);
        Object.DestroyImmediate(vScrollbar.GetComponent<Scrollbar>());
        var vScrollSlider = vScrollbar.AddComponent<Slider>();
        vScrollSlider.handleRect = handleRT;
        vScrollSlider.direction = Slider.Direction.BottomToTop;
        vScrollSlider.maxValue = 1;
        vScrollSlider.minValue = 0;
        vScrollSlider.value = 1;

        #endregion
        #region Horizontal "Scrollbar"

        var hScrollbar = Object.Instantiate(vScrollbar, scrollPane.transform, false);
        hScrollbar.name = "Scrollbar H";
        var hScrollbarRT = (RectTransform)hScrollbar.transform;
        hScrollbarRT.anchorMin = hScrollbarRT.anchorMax = new Vector2(0.5f, 0);
        hScrollbarRT.pivot = new Vector2(1, 0.5f);
        hScrollbar.AddComponent<RotatedParentSizeFitter>().fitToWidth = true;

        var hScrollSlider = hScrollbar.GetComponent<Slider>();
        hScrollSlider.direction = Slider.Direction.TopToBottom;
        hScrollSlider.value = 0.5f;

        #endregion

        var scrollRect = scrollPane.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 80;
        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;
        scrollRect.normalizedPosition = new Vector2(0.5f, 1);

        var sliderController = scrollPane.AddComponent<ScrollSliderController>();
        sliderController.VerticalSlider = vScrollSlider;
        sliderController.HorizontalSlider = hScrollSlider;

        scrollPane.AddComponent<ScrollFocusController>();

        return scrollPane;
    }

    private static readonly EventSuppressor mappableKeyInit = new();

    private static ReturnFlow MappableKeyInit(MappableKey self) =>
        mappableKeyInit.Suppressed ? ReturnFlow.SkipOriginal : ReturnFlow.None;

    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.MappableKey.OnEnable.ControlFlowPrefix(MappableKeyInit);
        Md.MappableKey.SetupRefs.ControlFlowPrefix(MappableKeyInit);
        Md.MappableKey.Start.ControlFlowPrefix(MappableKeyInit);
    }
}
