using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

// Largely copied from https://github.com/homothetyhk/HollowKnight.MenuChanger/blob/master/MenuChanger/CustomInputField.cs
internal class CustomInputField : InputField
{
    private static readonly HashSet<KeyCode> navigationCodes =
    [
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
    ];

    private RectTransform? textRect;
    private readonly Event processingEvent = new();

    private new void Awake()
    {
        base.Awake();
        textRect = textComponent.gameObject.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (textRect == null)
            return;

        var width = Mathf.Max(200, preferredWidth);
        textRect.offsetMin = new(-width, 0);
        textRect.sizeDelta = new(width, 0);
    }

    private bool AllSelected() =>
        caretPositionInternal == text.Length && caretSelectPositionInternal == 0;

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (!isFocused)
            return;

        bool allSelected = AllSelected();
        bool updateLabel = false;
        while (Event.PopEvent(processingEvent))
        {
            if (processingEvent.rawType == EventType.KeyDown)
            {
                updateLabel = true;
                if (allSelected && navigationCodes.Contains(processingEvent.keyCode))
                {
                    updateLabel = false;
                    DeactivateInputField();
                    break;
                    // stop processing events, because we are about to deselect
                }
                if (KeyPressed(processingEvent) == InputField.EditState.Finish)
                {
                    DeactivateInputField();
                    break;
                }
            }

            if (processingEvent.type <= EventType.ExecuteCommand)
            {
                string commandName = processingEvent.commandName;
                if (commandName != null && commandName == "SelectAll")
                {
                    SelectAll();
                    updateLabel = true;
                }
            }
        }

        if (updateLabel)
            UpdateLabel();
        if (!allSelected)
            eventData.Use();
    }
}
