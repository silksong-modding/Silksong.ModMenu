using System.Collections.Generic;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Examples;

internal class ChestButton : TextButton
{
    private readonly string closedText;
    private readonly string openedText;

    private bool opened = false;
    private bool abandoned = false;

    internal ChestButton(string closed, string open)
        : base(closed)
    {
        closedText = closed;
        openedText = open;

        OnSubmit += () =>
        {
            if (opened)
                return;

            opened = true;
            ButtonText.text = open;
        };
    }

    private void Open()
    {
        if (opened || abandoned)
            return;

        opened = true;
        ButtonText.text = openedText;
    }

    private void Abandon()
    {
        if (opened || abandoned)
            return;

        abandoned = true;
        ButtonText.text = "Chest abandoned.";
        Interactable = false;
    }

    internal void Reset()
    {
        ButtonText.text = closedText;
        Interactable = true;
        opened = false;
        abandoned = false;
    }

    public static void SynchronizeGroup(List<ChestButton> chests)
    {
        foreach (var chest in chests)
        {
            var copy = chest;
            chest.OnSubmit += () =>
            {
                copy.Open();
                foreach (var other in chests)
                    other.Abandon();
            };
        }
    }
}
