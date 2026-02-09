using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using MonoDetour;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using Silksong.ModMenu.Internal;
using Steamworks;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// API for changing menu screens while the player is navigating menus.
/// </summary>
[MonoDetourTargets(typeof(GameManager))]
[MonoDetourTargets(typeof(UIManager), GenerateControlFlowVariants = true)]
public static class MenuScreenNavigation
{
    /// <summary>
    /// Used for events, to indicate which direction menu navigation is going.
    /// </summary>
    public enum NavigationType
    {
        /// <summary>
        /// Indicating this screen is being entered/exited in order to open a *new* screen.
        /// </summary>
        Forwards,

        /// <summary>
        /// Indicating this screen is being entered/exited in order to open a *previous* screen.
        /// </summary>
        Backwards,
    }

    private static (MainMenuState, MenuScreen)? lastBaseMenuScreen;
    private static readonly Stack<AbstractMenuScreen> history = [];

    /// <summary>
    /// Get the current custom mod screen currently visible, if any.
    /// </summary>
    public static AbstractMenuScreen? CurrentModMenuScreen
    {
        get => history.Count > 0 ? history.Peek() : null;
    }

    /// <summary>
    /// Switch from the current menu screen to the provided custom menu screen.
    /// </summary>
    public static void Show(
        AbstractMenuScreen customMenuScreen,
        HistoryMode historyMode = HistoryMode.Add
    )
    {
        if (history.Count == 0)
            CaptureBaseMenuState();
        else if (history.Peek() == customMenuScreen)
        {
            // We're already showing this screen.
            return;
        }

        var ui = UIManager.instance;
        IEnumerator Routine()
        {
            yield return ui.StartCoroutine(HideCurrentMenu(NavigationType.Forwards));
            customMenuScreen.InvokeOnShow(NavigationType.Forwards);
            yield return ui.StartCoroutine(ui.ShowMenu(customMenuScreen.MenuScreen));

            // Replace the previous menu.
            if (historyMode == HistoryMode.Replace && history.Count > 0)
                history.Pop();

            history.Push(customMenuScreen);
        }
        ui.StartCoroutine(Routine());
    }

    /// <summary>
    /// Go back N screens, or until reaching a vanilla screen. By default, N=1.
    /// </summary>
    public static void GoBack(int count = 1)
    {
        int[] arr = [count];
        GoBackWhile(_ => --arr[0] >= 0);
    }

    /// <summary>
    /// Go back until reaching the specified menu screen, or a vanilla screen.
    /// </summary>
    /// <param name="customMenuScreen"></param>
    public static void GoBackTo(AbstractMenuScreen customMenuScreen) =>
        GoBackWhile(c => c != customMenuScreen);

    /// <summary>
    /// Go back through the linear history of custom menu screens until reaching a vanilla screen, or until the filter returns false.
    /// </summary>
    /// <param name="filter"></param>
    public static void GoBackWhile(Func<AbstractMenuScreen, bool> filter)
    {
        AbstractMenuScreen? toHide = null;
        bool anyPop = false;
        while (history.Count > 0 && filter(history.Peek()))
        {
            toHide = history.Pop();
            anyPop = true;
        }
        if (!anyPop)
            return;

        var ui = UIManager.instance;
        IEnumerator Routine()
        {
            yield return ui.StartCoroutine(
                ui.HideMenu(toHide!.MenuScreen)
                    .WithContext<NavigationTypeContext>(new(NavigationType.Backwards))
            );

            if (history.Count > 0)
            {
                history.Peek().InvokeOnShow(NavigationType.Backwards);
                yield return ui.StartCoroutine(ui.ShowMenu(history.Peek().MenuScreen));
            }
            else if (lastBaseMenuScreen.HasValue)
            {
                var (state, screen) = lastBaseMenuScreen.Value;
                lastBaseMenuScreen = null;

                yield return ui.StartCoroutine(ui.ShowMenu(screen));
                ui.menuState = state;
            }
            else
            {
                ModMenuPlugin.LogWarning("Navigation error; going back to Options");

                yield return ui.StartCoroutine(ui.ShowMenu(ui.optionsMenuScreen));
                ui.menuState = MainMenuState.OPTIONS_MENU;
            }
        }
        ui.StartCoroutine(Routine());
    }

    private static void CaptureBaseMenuState()
    {
        var ui = UIManager.instance;
        var state = ui.menuState;
        var screen = ui.GetActiveMenuScreen();

        if (screen != null)
            lastBaseMenuScreen = (state, screen);
    }

    private static IEnumerator HideCurrentMenu(NavigationType navigationType) =>
        UIManager
            .instance.HideCurrentMenu()
            .WithContext<NavigationTypeContext>(new(navigationType));

    private static ReturnFlow OverrideUIGoBack(UIManager self, ref bool returnValue)
    {
        if (history.Count == 0)
            return ReturnFlow.None;

        history.Peek().InvokeOnGoBack();
        returnValue = true;
        return ReturnFlow.SkipOriginal;
    }

    private class NavigationTypeContext(NavigationType navigationType)
    {
        internal readonly NavigationType NavigationType = navigationType;
    }

    private static void OnHideMenu(MenuScreen menuScreen)
    {
        if (!menuScreen.gameObject.TryGetComponent<AbstractMenuScreen.Marker>(out var marker))
            return;

        var screen = marker.AbstractMenuScreen;
        if (screen == null)
            return;

        // Invoke the hide event.
        if (ThreadLocalContext<NavigationTypeContext>.Get(out var context))
        {
            screen.InvokeOnHide(context.NavigationType);

            // Update history only if going backwards.
            if (
                context.NavigationType == NavigationType.Backwards
                && history.Count > 0
                && history.Peek() == screen
            )
                history.Pop();
        }
        else
        {
            screen.InvokeOnHide(NavigationType.Backwards);
            // We've been wiped externally.
            history.Clear();
        }
    }

    private static ReturnFlow PrefixHideCurrentMenu(UIManager self, ref IEnumerator returnValue)
    {
        if (history.Count == 0)
            return ReturnFlow.None;

        var screen = history.Peek();
        IEnumerator Routine()
        {
            var ui = UIManager.instance;
            ui.isFadingMenu = true;
            yield return ui.StartCoroutine(ui.HideMenu(screen.MenuScreen));
            ui.isFadingMenu = false;
        }

        returnValue = Routine();
        return ReturnFlow.SkipOriginal;
    }

    private static void PostfixHideMenu(
        UIManager self,
        ref MenuScreen menuScreen,
        ref bool disable,
        ref IEnumerator returnValue
    )
    {
        var localCopy = menuScreen;
        returnValue = returnValue.Append(() => OnHideMenu(localCopy));
    }

    private static void PostfixHideMenuInstant(UIManager self, ref MenuScreen menuScreen) =>
        OnHideMenu(menuScreen);

    private static void PrefixUIOnDestroy(UIManager self)
    {
        if (history.Count > 0)
            UIManager.instance.HideMenuInstant(history.Peek().MenuScreen);
    }

    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.UIManager.UIGoBack.ControlFlowPrefix(OverrideUIGoBack);
        Md.UIManager.HideCurrentMenu.ControlFlowPrefix(PrefixHideCurrentMenu);
        Md.UIManager.HideMenu.Postfix(PostfixHideMenu);
        Md.UIManager.HideMenuInstant.Postfix(PostfixHideMenuInstant);
        Md.UIManager.OnDestroy.Prefix(PrefixUIOnDestroy);
    }
}
