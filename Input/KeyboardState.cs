// File: MUIBridge/Input/KeyboardState.cs
// Purpose: Thread-safe keyboard state tracking (ported from DUSK).

using System.Collections.Concurrent;

namespace MUIBridge.Input
{
    /// <summary>
    /// Thread-safe keyboard state tracking.
    /// Tracks pressed keys, modifiers, and key hold durations.
    /// </summary>
    public class KeyboardState
    {
        private readonly ConcurrentDictionary<Keys, DateTime> _pressedKeys = new();
        private volatile KeyModifiers _modifiers;

        /// <summary>
        /// Gets the currently pressed modifier keys.
        /// </summary>
        public KeyModifiers Modifiers => _modifiers;

        /// <summary>
        /// Gets whether any key is currently pressed.
        /// </summary>
        public bool AnyKeyPressed => !_pressedKeys.IsEmpty;

        /// <summary>
        /// Gets all currently pressed keys.
        /// </summary>
        public IReadOnlyCollection<Keys> PressedKeys => _pressedKeys.Keys.ToArray();

        /// <summary>
        /// Checks if a specific key is currently pressed.
        /// </summary>
        public bool IsKeyDown(Keys key) => _pressedKeys.ContainsKey(key);

        /// <summary>
        /// Checks if a specific key is not pressed.
        /// </summary>
        public bool IsKeyUp(Keys key) => !_pressedKeys.ContainsKey(key);

        /// <summary>
        /// Gets how long a key has been held down.
        /// </summary>
        public TimeSpan GetKeyHoldDuration(Keys key)
        {
            if (_pressedKeys.TryGetValue(key, out var pressTime))
            {
                return DateTime.UtcNow - pressTime;
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Checks if Shift is pressed.
        /// </summary>
        public bool IsShiftDown => (_modifiers & KeyModifiers.Shift) != 0;

        /// <summary>
        /// Checks if Control is pressed.
        /// </summary>
        public bool IsControlDown => (_modifiers & KeyModifiers.Control) != 0;

        /// <summary>
        /// Checks if Alt is pressed.
        /// </summary>
        public bool IsAltDown => (_modifiers & KeyModifiers.Alt) != 0;

        internal void SetKeyDown(Keys key)
        {
            _pressedKeys.TryAdd(key, DateTime.UtcNow);
            UpdateModifiers(key, true);
        }

        internal void SetKeyUp(Keys key)
        {
            _pressedKeys.TryRemove(key, out _);
            UpdateModifiers(key, false);
        }

        internal void Clear()
        {
            _pressedKeys.Clear();
            _modifiers = KeyModifiers.None;
        }

        private void UpdateModifiers(Keys key, bool isDown)
        {
            KeyModifiers mod = key switch
            {
                Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey => KeyModifiers.Shift,
                Keys.ControlKey or Keys.LControlKey or Keys.RControlKey => KeyModifiers.Control,
                Keys.Menu or Keys.LMenu or Keys.RMenu => KeyModifiers.Alt,
                _ => KeyModifiers.None
            };

            if (mod != KeyModifiers.None)
            {
                if (isDown)
                    _modifiers |= mod;
                else
                    _modifiers &= ~mod;
            }
        }
    }

    /// <summary>
    /// Keyboard modifier flags.
    /// </summary>
    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4,
        Super = 8  // Windows key
    }
}
