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
    private ShortcutRecording? pendingRecording;

    /// <summary>
    /// Start listening, ignoring key downs that already happened.
    /// </summary>
    public void Reset()
    {
        var heldNow = HeldKeys();
        previousHeldKeys = [.. heldNow];
        pendingRecording = null;
    }

    /// <summary>
    /// Returns the recorded shortcut when finished
    /// </summary>
    public ShortcutRecording? Listen()
    {
        var held = HeldKeys();

        // If a shortcut was recorded, wait for key up until it is confirmed
        if (pendingRecording is { } pending)
        {
            if (held.Contains(pending.MainKey))
                return null;

            pendingRecording = null;
            return pending;
        }

        // On key down, store the current main keys along with its modifiers in pendingRecording
        var regularKeyDown = regularKeys.FirstOrDefault(k =>
            held.Contains(k) && !previousHeldKeys.Contains(k)
        );
        if (regularKeyDown != Key.None)
        {
            pendingRecording = new ShortcutRecording(
                regularKeyDown,
                [.. modifierKeys.Where(held.Contains)]
            );
            return null;
        }

        // A single modifier press can be recorded as a keybind
        var keyUp =
            held.Count == 0 && previousHeldKeys.Count == 1 ? previousHeldKeys.First() : Key.None;
        if (keyUp != Key.None && modifierKeys.Contains(keyUp))
        {
            return new ShortcutRecording(keyUp, []);
        }

        // Every frame when the shortcut isn't recorded yet, snapshot the held keys for the next frame.
        previousHeldKeys = held;
        return null;
    }

    private static HashSet<Key> HeldKeys() =>
        [.. regularKeys.Concat(modifierKeys).Where(InputManager.KeyboardProvider.GetKeyIsPressed)];
}
