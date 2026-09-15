using System;
using System.Collections.Generic;
using System.Linq;
using InControl;

namespace Silksong.ModMenu.Internal;

internal readonly record struct ShortcutRecording(Key MainKey, Key[] Modifiers);

/// <summary>
/// Like <see cref="KeyBindingSourceListener"/>, but for capturing
/// full keyboard shortcuts with modifiers.
/// </summary>
internal class KeyShortcutSourceListener
{
    /// <summary>
    /// All keys that can act as the main key of a shortcut
    /// </summary>
    private static readonly IReadOnlyList<Key> regularKeys =
    [
        .. Enum.GetValues(typeof(Key)).Cast<Key>().Where(k => k >= Key.Escape),
    ];

    private static readonly Key[] modifierKeys =
    [
        Key.LeftShift,
        Key.LeftAlt,
        Key.LeftCommand,
        Key.LeftControl,
        Key.RightShift,
        Key.RightAlt,
        Key.RightCommand,
        Key.RightControl,
    ];

    private HashSet<Key> previousHeldKeys = [];
    private HashSet<Key> startHeldKeys = [];
    private (Key key, Key[] modifiers)? pendingRecording;

    /// <summary>
    /// Start listening, ignoring the keys currently held.
    /// </summary>
    public void Reset()
    {
        var heldNow = HeldKeys();
        previousHeldKeys = [.. heldNow];
        startHeldKeys = [.. heldNow];
        pendingRecording = null;
    }

    /// <summary>
    /// Returns the recorded shortcut when finished
    /// </summary>
    public ShortcutRecording? Listen()
    {
        var held = HeldKeys();

        // Confirm on keyup, like single keys
        if (pendingRecording is { } pending)
        {
            if (held.Contains(pending.key))
                return null;

            pendingRecording = null;
            return new(pending.key, pending.modifiers);
        }

        var pressedKey = regularKeys.FirstOrDefault(k =>
            held.Contains(k) && !previousHeldKeys.Contains(k)
        );
        if (pressedKey != Key.None)
        {
            pendingRecording = (pressedKey, [.. modifierKeys.Where(held.Contains)]);
            return null;
        }

        // A single modifier press
        if (
            held.Count == 0
            && previousHeldKeys.Count == 1
            && modifierKeys.Contains(previousHeldKeys.First())
            && !startHeldKeys.Contains(previousHeldKeys.First())
        )
        {
            var modifier = previousHeldKeys.First();
            return new(modifier, []);
        }

        previousHeldKeys = held;
        startHeldKeys.IntersectWith(held); // Make keys held at start capturable when released
        return null;
    }

    private static HashSet<Key> HeldKeys() =>
        [.. regularKeys.Concat(modifierKeys).Where(InputManager.KeyboardProvider.GetKeyIsPressed)];
}
