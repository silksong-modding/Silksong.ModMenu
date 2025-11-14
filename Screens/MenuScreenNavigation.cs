using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using MonoDetour;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using Silksong.ModMenu.Internal;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// API for changing menu screens while the player is navigating menus.
/// </summary>
[MonoDetourTargets(typeof(UIManager), GenerateControlFlowVariants = true)]
[MonoDetourTargets(typeof(GameManager))]
public static class MenuScreenNavigation
{
    /// <summary>
    /// Used for events, to indicate which direction menu navigation is going.
    /// </summary>
    public enum NavigationType
    {
        Forwards,
        Backwards,
    }

    private static (MainMenuState, MenuScreen)? lastBaseMenuScreen;
    private static readonly Stack<AbstractMenuScreen> history = [];

    /// <summary>
    /// Switch from the current menu screen to the provided custom menu screen.
    /// </summary>
    public static void Show(
        AbstractMenuScreen customMenuScreen,
        HistoryMode historyMode = HistoryMode.Add
    )
    {
        AbstractMenuScreen? toHide = null;
        if (history.Count == 0)
            CaptureBaseMenuState();
        else if (history.Peek() == customMenuScreen)
        {
            // We're already showing this screen.
            return;
        }
        else
            toHide = history.Peek();

        var ui = UIManager.instance;
        IEnumerator Routine()
        {
            yield return ui.StartCoroutine(HideCurrentMenu(ui, toHide, NavigationType.Forwards));
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
        AbstractMenuScreen? toHide = history.Count > 0 ? history.Peek() : null;

        bool anyPop = false;
        while (history.Count > 0 && filter(history.Peek()))
        {
            history.Pop();
            anyPop = true;
        }
        if (!anyPop)
            return;

        var ui = UIManager.instance;
        IEnumerator Routine()
        {
            yield return ui.StartCoroutine(HideCurrentMenu(ui, toHide, NavigationType.Backwards));

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

    private static IEnumerator HideCurrentMenu(
        UIManager ui,
        AbstractMenuScreen? screen,
        NavigationType navigationType
    )
    {
        if (screen == null)
            return ui.HideCurrentMenu();

        IEnumerator Routine()
        {
            var ui = UIManager.instance;
            ui.SetIsFadingMenu(true);
            yield return ui.StartCoroutine(ui.HideMenu(screen.MenuScreen, true));

            ui.SetIsFadingMenu(false);
            screen?.InvokeOnHide(navigationType);
        }
        return Routine();
    }

    private static void HideMenuInstant(AbstractMenuScreen screen)
    {
        screen.InvokeOnHide(NavigationType.Backwards);
        UIManager.instance.HideMenuInstant(screen.MenuScreen);
    }

    private static void HideMenus()
    {
        if (history.Count == 0)
            return;

        HideMenuInstant(history.Peek());
        history.Clear();
    }

    private static void PrefixGameManagerAwake(GameManager self)
    {
        self.GamePausedChange += isPaused =>
        {
            if (!isPaused)
                HideMenus();
        };
    }

    private static ReturnFlow OverrideUIGoBack(UIManager self, ref bool returnValue)
    {
        if (history.Count == 0)
            return ReturnFlow.None;

        HideMenuInstant(history.Peek());
        return ReturnFlow.SkipOriginal;
    }

    private static void PrefixUIOnDestroy(UIManager self)
    {
        if (UIManager.instance == self)
            HideMenus();
    }

    [MonoDetourHookInitialize]
    private static void Hook()
    {
        Md.GameManager.Awake.Prefix(PrefixGameManagerAwake);
        Md.UIManager.UIGoBack.ControlFlowPrefix(OverrideUIGoBack);
        Md.UIManager.OnDestroy.Prefix(PrefixUIOnDestroy);
    }
}
