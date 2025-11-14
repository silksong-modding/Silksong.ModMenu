using System;
using System.Collections;
using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using Silksong.UnityHelper.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A single menu page with a title amd a back button.
/// Concrete subclasses are responsible for controlling layout, navigation, and adding elements.
/// </summary>
public abstract class AbstractMenuScreen : MenuDisposable
{
    #region Constructor
    protected AbstractMenuScreen(string title)
    {
        Container = MenuPrefabs.Get().NewCustomMenu(title);
        MenuScreen = Container.GetComponent<MenuScreen>();
        TitleText = Container.FindChild("Title")!.GetComponent<Text>();
        ContentPane = Container!.FindChild("Content")!;
        ControlsPane = Container!.FindChild("Controls")!;
        BackButton = ControlsPane!.FindChild("ApplyButton")!.GetComponent<MenuButton>();

        BackButton.gameObject.GetComponent<EventTrigger>().SetCallback(OnGoBackImpl);
        MenuScreen.backButton = BackButton;

        Container.AddComponent<OnDestroyHelper>().Action += Dispose;
        TitleText.text = title;
    }
    #endregion

    #region Components
    /// <summary>
    /// The actual GameObject containing all the elements of this custom menu.
    /// </summary>
    public readonly GameObject Container;

    /// <summary>
    /// The MenuScreen component of this GameObject.
    /// </summary>
    public readonly MenuScreen MenuScreen;

    /// <summary>
    /// The text element for the title of this menu screen.
    /// </summary>
    public readonly Text TitleText;

    /// <summary>
    /// The GameObject containing all menu elements except the back button, which is always at the bottom.
    /// </summary>
    public readonly GameObject ContentPane;

    /// <summary>
    /// The GameObject containing the back button, and any additional navigational elements specific to the layout.
    /// </summary>
    public readonly GameObject ControlsPane;

    /// <summary>
    /// The 'Back' button at the bottom of the menu screen.
    /// </summary>
    public readonly MenuButton BackButton;
    #endregion

    #region Events
    /// <summary>
    /// Additional actions taken when navigating to this menu.
    /// These are invoked immediately prior to the menu fading in.
    /// </summary>
    public event Action<MenuScreenNavigation.NavigationType>? OnShow;

    /// <summary>
    /// If false, do not navigate to the previous menu screen when the user selects the back button.
    /// This allows the client to indicate that an alternative action will be taken instead.
    /// </summary>
    public bool AllowGoBack = true;

    /// <summary>
    /// Additional actions to perform when navigating back from the current menu screen.
    /// These are invoked immediately on button-press. If you want to quietly reset values after the transition fade ends, use OnHide instead.
    /// </summary>
    public event Action? OnGoBack;

    /// <summary>
    /// Additional actions to perform when navigating back from the current menu screen.
    /// These are invoked after the transition fade completes, so it is not a suitable place to provide custom go-back behavior.
    /// </summary>
    public event Action<MenuScreenNavigation.NavigationType>? OnHide;
    #endregion

    #region Layout
    /// <summary>
    /// Invoke the delegate to update the menu layout. Prevents re-entry caused by listeners.
    /// </summary>
    protected abstract void UpdateLayout();

    /// <summary>
    /// Helper to update the layout when a relevant property is modified.
    /// </summary>
    protected void MaybeUpdateLayout<T>(ref T self, T value)
    {
        if (EqualityComparer<T>.Default.Equals(self, value))
            return;

        self = value;
        UpdateLayout();
    }
    #endregion

    #region SelectOnShow
    /// <summary>
    /// Record that this selectable element is part of the menu screen. This is used for SelectOnShow.
    /// </summary>
    protected void RegisterSelectable(SelectableElement selectableElement) =>
        selectableElement
            .SelectableComponent.gameObject.GetOrAddComponent<OnSelectHelper>()
            .OnSelect += () => lastSelected = selectableElement;

    /// <summary>
    /// Standard behaviours for which element to select when showing the menu screen.
    /// </summary>
    public SelectOnShowBehaviour SelectOnShowBehaviour = SelectOnShowBehaviour.ForgetBackwards;

    /// <summary>
    /// Determine which selectable to select when showing this menu screen.
    /// </summary>
    protected virtual Selectable SelectOnShow(MenuScreenNavigation.NavigationType navigationType)
    {
        return SelectOnShowBehaviour switch
        {
            SelectOnShowBehaviour.AlwaysForget => GetDefaultSelectable(),
            SelectOnShowBehaviour.ForgetBackwards => navigationType
            == MenuScreenNavigation.NavigationType.Forwards
                ? GetDefaultSelectable()
                : GetLastSelectableOrDefault(),
            SelectOnShowBehaviour.NeverForget => GetLastSelectableOrDefault(),
            _ => GetDefaultSelectable(),
        };
    }
    #endregion

    #region Internal
    internal void InvokeOnShow(MenuScreenNavigation.NavigationType navigationType)
    {
        UIManager.instance.StartCoroutine(SelectDelayed(SelectOnShow(navigationType)));
        OnShow?.Invoke(navigationType);
    }

    private IEnumerator SelectDelayed(Selectable selectable)
    {
        yield return new WaitUntil(() => selectable.gameObject.activeInHierarchy);
        selectable.ForceSelect();
    }

    private void OnGoBackImpl()
    {
        if (
            !ExceptionUtil.Try(() => OnGoBack?.Invoke(), "Custom GoBack action failed")
            || AllowGoBack
        )
            MenuScreenNavigation.GoBack();
    }

    internal void InvokeOnHide(MenuScreenNavigation.NavigationType navigationType) =>
        OnHide?.Invoke(navigationType);

    private SelectableElement? lastSelected;

    /// <summary>
    /// Get the default selectable to choose when showing this menu.
    /// </summary>
    protected abstract SelectableElement? GetDefaultSelectableInternal();

    private Selectable GetDefaultSelectable()
    {
        var s = GetDefaultSelectableInternal();
        return (s != null && s.Visible && s.Interactable) ? s.SelectableComponent : BackButton;
    }

    protected Selectable GetLastSelectableOrDefault() =>
        (lastSelected != null && lastSelected.Visible && lastSelected.Interactable)
            ? lastSelected.SelectableComponent
            : GetDefaultSelectable();
    #endregion
}
