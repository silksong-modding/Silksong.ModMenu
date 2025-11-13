using System.Collections.Generic;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Silksong.ModMenu.Examples;

internal enum Direction
{
    North,
    East,

    [ModMenuIgnore]
    EastWest,

    [ModMenuName("Not East")]
    West,
    South,
}

internal static class DemoTextAdventure
{
    internal static SimpleMenuScreen BuildMenu()
    {
        string title = "You find yourself in a dark forest";
        SimpleMenuScreen menu = new(title);

        ChoiceElement<Direction> whichWay = new(
            "Which way do you go?",
            ChoiceModels.ForEnum<Direction>(),
            "A m00se bit my sister once."
        );
        menu.Add(whichWay);

        string baseDesc = "How many mph?  (Speed limit 15)";
        SliderElement<int> mph = new(baseDesc, SliderModels.ForInts(4, 20));
        mph.LabelText.fontSize = FontSizeConstants.LABEL_SMALL;
        mph.OnValueChanged += v =>
            mph.State = v <= 15 ? ElementState.DEFAULT : ElementState.INVALID;
        menu.Add(mph);

        TextButton go = new("Go");
        mph.OnValueChanged += v =>
        {
            if (v <= 8)
                go.ButtonText.text = "Go";
            else if (v <= 12)
                go.ButtonText.text = "Run";
            else if (v <= 15)
                go.ButtonText.text = "Sprint";
            else
            {
                go.ButtonText.text = "You're going too fast!";
                go.State = ElementState.INVALID;
                go.Interactable = false;
            }

            if (v <= 15)
            {
                go.State = ElementState.DEFAULT;
                go.Interactable = true;
            }
        };
        mph.OnStateChanged += s => go.State = s;
        menu.Add(go);

        TextButton stay = new("Stay")
        {
            OnSubmit = () =>
            {
                if (menu.TitleText.text.EndsWith("..."))
                    menu.TitleText.text += "...";
                else
                    menu.TitleText.text = "It's getting very quiet...";
            },
        };
        menu.Add(stay);

        void Reset()
        {
            menu.TitleText.text = title;
            whichWay.Value = Direction.North;
            mph.Value = 6;
        }

        Reset();
        menu.OnHide += _ => Reset();

        var north = North();
        var south = South();
        var eastSlow = EastSlow();
        var eastFast = EastFast();
        go.OnSubmit += () =>
        {
            switch (whichWay.Value)
            {
                case Direction.North:
                    MenuScreenNavigation.Show(north);
                    break;
                case Direction.South:
                    MenuScreenNavigation.Show(south);
                    break;
                case Direction.East:
                    MenuScreenNavigation.Show(mph.Value >= 12 ? eastFast : eastSlow);
                    break;
                case Direction.West:
                    menu.TitleText.text = "There is no west.";
                    break;
            }
        };

        return menu;
    }

    private static SimpleMenuScreen North()
    {
        SimpleMenuScreen menu = new("You have been slain by") { AllowGoBack = false };
        menu.OnGoBack += () => MenuScreenNavigation.GoBack(2);

        List<TextButton> kills = [];
        kills.Add(new("A Primal Aspid"));
        kills.Add(new("The Void"));
        kills.Add(new("Bilewater"));
        menu.Add(kills);

        foreach (var kill in kills)
            kill.OnSubmit += () => MenuScreenNavigation.GoBack(2);
        return menu;
    }

    private static SimpleMenuScreen South()
    {
        SimpleMenuScreen menu = new("You found three chests!  Open one.") { AllowGoBack = false };
        menu.OnGoBack += () => MenuScreenNavigation.GoBack(2);

        List<ChestButton> chests = [];
        chests.Add(new("This one's covered in webs.", "You found a Silkeater"));
        chests.Add(new("Thie one is plated in gold!", "It's filled with a dark poison."));
        chests.Add(
            new("This one is already empty?", "It had a false bottom, with 50 rosaries beneath!")
        );
        menu.Add(chests);
        ChestButton.SynchronizeGroup(chests);

        menu.OnHide += _ =>
        {
            foreach (var chest in chests)
                chest.Reset();
        };
        return menu;
    }

    private static SimpleMenuScreen EastSlow()
    {
        SimpleMenuScreen menu = new("You fell into a ravine!") { AllowGoBack = false };
        menu.OnGoBack += () => MenuScreenNavigation.GoBack(2);

        SliderElement<string> whoops = new("Whoops", SliderModels.ForValues(["A", "AA"]));
        menu.Add(whoops);

        ChoiceElement<string> choice = new(
            "This is unfortunate",
            ChoiceModels.ForValues(["Gotta", "Go", "Faster", "Next", "Time"])
        );
        menu.Add(choice);

        return menu;
    }

    private static SimpleMenuScreen EastFast()
    {
        SimpleMenuScreen menu = new("You jumped a ravine!") { AllowGoBack = false };
        menu.OnGoBack += () => MenuScreenNavigation.GoBack(2);

        menu.Add(new TextButton("Congratulations!"));

        int[] rosaries = [0];
        string diceText = "Roll some dice for your troubles.";
        TextButton dice = new(diceText);
        dice.OnSubmit += () =>
        {
            if (rosaries[0] > 0 && UnityEngine.Random.Range(0, 3) == 0)
            {
                rosaries[0] = 0;
                dice.ButtonText.text = "You lost all your rosaries.";
            }
            else
            {
                rosaries[0] += 4;
                dice.ButtonText.text =
                    rosaries[0] > 4
                        ? $"You won 4 rosaries! Now you have {rosaries[0]}."
                        : "You won 4 rosaries!";
            }
        };

        menu.OnShow += _ =>
        {
            dice.ButtonText.text = diceText;
            rosaries[0] = 0;
        };
        return menu;
    }
}
