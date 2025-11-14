using System.Collections.Generic;
using System.Linq;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.ModMenu.Screens;

/// <summary>
/// A simple menu screen with a single column of elements, with no pagination or scrolling.
/// </summary>
public class SimpleMenuScreen(string title) : AbstractMenuScreen(title)
{
    private readonly List<MenuElement> menuElements = [];

    /// <summary>
    /// The full list of elements in this screen.
    /// </summary>
    public IReadOnlyList<MenuElement> MenuElements => menuElements;

    /// <summary>
    /// The number of elements in this screen.
    /// </summary>
    public int Count => menuElements.Count;

    /// <summary>
    /// Top anchor point for the elements.
    /// </summary>
    public Vector2 Anchor
    {
        get => field;
        set => MaybeUpdateLayout(ref field, value);
    } = SpacingConstants.TOP_CENTER_ANCHOR;

    /// <summary>
    /// Vertical space between rendered elements.
    /// </summary>
    public float VerticalSpacing
    {
        get => field;
        set => MaybeUpdateLayout(ref field, value);
    } = SpacingConstants.VSPACE_MEDIUM;

    /// <summary>
    /// If true, hide move lower elements upwards when space is made for them by inactive elements above.
    /// </summary>
    public bool HideInactiveElements
    {
        get => field;
        set => MaybeUpdateLayout(ref field, value);
    } = true;

    public void Add(MenuElement element) => Add([element]);

    public void Add(IEnumerable<MenuElement> elements)
    {
        List<MenuElement> toAdd = [.. elements];
        menuElements.AddRange(toAdd);

        foreach (var element in toAdd)
        {
            var elementCopy = element;
            element.OnVisibilityChanged += _ => UpdateLayout();
            if (element is SelectableElement s)
            {
                s.OnInteractableChanged += _ => UpdateLayout();
                RegisterSelectable(s);
            }

            element.AddToContainer(ContentPane);
        }
        UpdateLayout();
    }

    protected override SelectableElement? GetDefaultSelectableInternal() =>
        menuElements
            .Select(e => e as SelectableElement)
            .Where(s => s != null && s.Visible && s.Interactable)
            .FirstOrDefault();

    protected override void UpdateLayout()
    {
        Vector2 pos = Anchor;
        foreach (var menuElement in MenuElements)
        {
            var rect = menuElement.Container.GetComponent<RectTransform>();
            rect.SetLocalPosition2D(0, 0);

            menuElement.RectTransform.SetAnchoredPosition(pos);
            if (menuElement.Visible || !HideInactiveElements)
                pos.y -= VerticalSpacing;
        }

        List<Selectable> selectables =
        [
            .. menuElements
                .Select(e => e as SelectableElement)
                .Where(s => s != null && s.Visible && s.Interactable)
                .Select(s => s!.SelectableComponent)
                .Concat([BackButton]),
        ];
        selectables.ForEach(s => s.ClearNav());
        foreach (var (top, bot) in selectables.CircularPairs())
        {
            top.SetNavDown(bot);
            bot.SetNavUp(top);
        }

        if (!selectables.Any(s => s.IsSelected()))
            GetLastSelectableOrDefault().ForceSelect();
    }
}
