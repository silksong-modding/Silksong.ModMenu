using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

internal class MenuPrefabs
{
    private static MenuPrefabs? instance;

    internal static MenuPrefabs Get() =>
        instance ?? throw new System.Exception("UIManager not initialized yet");

    internal static void Load(UIManager uiManager) => instance ??= new(uiManager);

    private readonly GameObject canvas;

    private readonly GameObject menuTemplate;
    private readonly GameObject emptyContentPane;
    private readonly GameObject textButtonTemplate;
    private readonly GameObject textLabelTemplate;
    private readonly GameObject textChoiceTemplate;
    private readonly GameObject sliderTemplate;

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
        Object.Destroy(menuTemplate.GetComponent<MenuButtonList>());
        Object.Destroy(menuTemplate.FindChild("Title")!.GetComponent<AutoLocalizeTextUI>());
        Object.DontDestroyOnLoad(menuTemplate);

        emptyContentPane = menuTemplate.FindChild("Content")!;
        emptyContentPane.DestroyAllChildren();
        Object.Destroy(emptyContentPane.GetComponent<VerticalLayoutGroup>());
        Object.Destroy(emptyContentPane.GetComponent<MenuButtonList>());
        Object.DontDestroyOnLoad(emptyContentPane);

        textButtonTemplate = Object.Instantiate(optionsScreen.FindChild("Content/GameOptions")!);
        textButtonTemplate.SetActive(false);
        textButtonTemplate.name = "TextButtonContainer";
        Object.DontDestroyOnLoad(textButtonTemplate);

        var buttonChild = textButtonTemplate.FindChild("GameOptionsButton")!;
        buttonChild.name = "TextButton";
        Object.Destroy(buttonChild.GetComponent<AutoLocalizeTextUI>());

        textLabelTemplate = Object.Instantiate(
            optionsScreen.FindChild("Content/GameOptions/GameOptionsButton/Menu Button Text")!
        );
        textLabelTemplate.SetActive(false);
        textLabelTemplate.name = "TextLabel";
        Object.DontDestroyOnLoad(textLabelTemplate);

        textChoiceTemplate = Object.Instantiate(
            canvas.FindChild("GameOptionsMenuScreen/Content/CamShakeSetting")!
        );
        textChoiceTemplate.SetActive(false);
        textChoiceTemplate.name = "ValueChoiceContainer";
        Object.DontDestroyOnLoad(textChoiceTemplate);

        var choiceChild = textChoiceTemplate.FindChild("CamShakePopupOption")!;
        choiceChild.name = "ValueChoice";
        Object.Destroy(choiceChild.GetComponent<MenuSetting>());
        var moh = choiceChild.GetComponent<MenuOptionHorizontal>();
        moh.optionList = ["###INTERNAL###"];
        moh.menuSetting = null;
        moh.localizeText = false;
        moh.applyButton = null;
        Object.Destroy(
            choiceChild.FindChild("Menu Option Label")!.GetComponent<AutoLocalizeTextUI>()
        );

        sliderTemplate = Object.Instantiate(
            canvas.FindChild("AudioMenuScreen/Content/MasterVolume")!
        );
        sliderTemplate.SetActive(false);
        sliderTemplate.name = "SliderContainer";
        Object.DontDestroyOnLoad(sliderTemplate);

        var sliderChild = sliderTemplate.FindChild("MasterSlider")!;
        sliderChild.name = "Slider";
        sliderChild.GetComponent<Slider>().onValueChanged = new();
        Object.Destroy(sliderChild.GetComponent<MenuAudioSlider>());
        sliderChild.GetOrAddComponent<SliderRightStickInput>();
        Object.Destroy(
            sliderChild.FindChild("Menu Option Label")!.GetComponent<AutoLocalizeTextUI>()
        );
        sliderChild.FindChild("MasterVolValue")!.name = "Value";
    }

    internal GameObject NewCustomMenu(string title)
    {
        var obj = Object.Instantiate(menuTemplate);
        obj.name = $"ModMenuScreen-{title}";
        obj.transform.SetParent(canvas.transform, false);
        obj.transform.localPosition = new(0, 10, 0);

        return obj;
    }

    internal GameObject NewEmptyContentPane() => Object.Instantiate(emptyContentPane);

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

    internal GameObject NewSliderContainer(out Slider slider)
    {
        var obj = Object.Instantiate(sliderTemplate);
        slider = obj.FindChild("Slider")!.GetComponent<Slider>();
        return obj;
    }
}
