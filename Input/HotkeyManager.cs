// File: MUIBridge/Input/HotkeyManager.cs
// Purpose: Global hotkey management with thread-safe registration (ported from DUSK).

using System.Collections.Concurrent;

namespace MUIBridge.Input
{
    /// <summary>
    /// Manages global hotkeys with thread-safe registration and dispatch.
    /// </summary>
    public class HotkeyManager
    {
        private readonly ConcurrentDictionary<HotkeyBinding, HotkeyAction> _hotkeys = new();
        private readonly ConcurrentDictionary<string, HotkeyBinding> _namedBindings = new();
        private volatile bool _enabled = true;

        /// <summary>
        /// Gets or sets whether hotkey processing is enabled.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>
        /// Gets all registered hotkey names.
        /// </summary>
        public IEnumerable<string> RegisteredHotkeys => _namedBindings.Keys;

        /// <summary>
        /// Registers a hotkey with an action.
        /// </summary>
        /// <param name="key">The key to bind.</param>
        /// <param name="modifiers">Required modifier keys.</param>
        /// <param name="action">Action to execute when hotkey is pressed.</param>
        /// <param name="name">Optional name for the hotkey.</param>
        public void Register(Keys key, KeyModifiers modifiers, Action action, string? name = null)
        {
            var binding = new HotkeyBinding(key, modifiers);
            var hotkeyAction = new HotkeyAction(action, name);

            _hotkeys.AddOrUpdate(binding, hotkeyAction, (_, _) => hotkeyAction);

            if (!string.IsNullOrEmpty(name))
            {
                _namedBindings.AddOrUpdate(name, binding, (_, _) => binding);
            }
        }

        /// <summary>
        /// Unregisters a hotkey by key and modifiers.
        /// </summary>
        public void Unregister(Keys key, KeyModifiers modifiers)
        {
            var binding = new HotkeyBinding(key, modifiers);
            if (_hotkeys.TryRemove(binding, out var action) && action.Name != null)
            {
                _namedBindings.TryRemove(action.Name, out _);
            }
        }

        /// <summary>
        /// Unregisters a hotkey by name.
        /// </summary>
        public void Unregister(string name)
        {
            if (_namedBindings.TryRemove(name, out var binding))
            {
                _hotkeys.TryRemove(binding, out _);
            }
        }

        /// <summary>
        /// Checks if a hotkey is registered.
        /// </summary>
        public bool IsRegistered(Keys key, KeyModifiers modifiers)
        {
            return _hotkeys.ContainsKey(new HotkeyBinding(key, modifiers));
        }

        /// <summary>
        /// Processes a key press and executes matching hotkey actions.
        /// </summary>
        /// <returns>True if a hotkey was matched and executed.</returns>
        public bool ProcessKeyPress(Keys key, KeyModifiers modifiers)
        {
            if (!_enabled) return false;

            var binding = new HotkeyBinding(key, modifiers);
            if (_hotkeys.TryGetValue(binding, out var action))
            {
                action.Action();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all registered hotkeys.
        /// </summary>
        public void Clear()
        {
            _hotkeys.Clear();
            _namedBindings.Clear();
        }

        /// <summary>
        /// Gets information about all registered hotkeys.
        /// </summary>
        public IReadOnlyList<HotkeyInfo> GetAllHotkeys()
        {
            return _hotkeys.Select(kvp => new HotkeyInfo(
                kvp.Key.Key,
                kvp.Key.Modifiers,
                kvp.Value.Name ?? "Unnamed"
            )).ToList();
        }
    }

    /// <summary>
    /// Represents a hotkey binding.
    /// </summary>
    public readonly record struct HotkeyBinding(Keys Key, KeyModifiers Modifiers);

    /// <summary>
    /// Represents a hotkey action.
    /// </summary>
    public readonly record struct HotkeyAction(Action Action, string? Name);

    /// <summary>
    /// Information about a registered hotkey.
    /// </summary>
    public readonly record struct HotkeyInfo(Keys Key, KeyModifiers Modifiers, string Name)
    {
        public override string ToString()
        {
            var parts = new List<string>();
            if ((Modifiers & KeyModifiers.Control) != 0) parts.Add("Ctrl");
            if ((Modifiers & KeyModifiers.Alt) != 0) parts.Add("Alt");
            if ((Modifiers & KeyModifiers.Shift) != 0) parts.Add("Shift");
            if ((Modifiers & KeyModifiers.Super) != 0) parts.Add("Win");
            parts.Add(Key.ToString());
            return $"{string.Join("+", parts)} -> {Name}";
        }
    }
}
