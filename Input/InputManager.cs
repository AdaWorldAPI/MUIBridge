// File: MUIBridge/Input/InputManager.cs
// Purpose: Centralized input handling for keyboard, mouse, and focus (ported from DUSK).

using System.Windows.Forms;

namespace MUIBridge.Input
{
    /// <summary>
    /// Centralized input manager for keyboard, mouse, focus, and hotkey handling.
    /// Provides a unified input pipeline for MUIBridge applications.
    /// </summary>
    public class InputManager : IDisposable
    {
        private static InputManager? _instance;
        private static readonly object _lock = new();

        private readonly KeyboardState _keyboardState = new();
        private readonly MouseState _mouseState = new();
        private readonly HotkeyManager _hotkeyManager = new();

        private Control? _focusedControl;
        private Control? _hoveredControl;
        private volatile bool _disposed;

        /// <summary>
        /// Gets the singleton instance of the InputManager.
        /// </summary>
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new InputManager();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the current keyboard state.
        /// </summary>
        public KeyboardState Keyboard => _keyboardState;

        /// <summary>
        /// Gets the current mouse state.
        /// </summary>
        public MouseState Mouse => _mouseState;

        /// <summary>
        /// Gets the hotkey manager.
        /// </summary>
        public HotkeyManager Hotkeys => _hotkeyManager;

        /// <summary>
        /// Gets the currently focused control.
        /// </summary>
        public Control? FocusedControl => _focusedControl;

        /// <summary>
        /// Gets the control currently under the mouse.
        /// </summary>
        public Control? HoveredControl => _hoveredControl;

        /// <summary>
        /// Raised when a key is pressed.
        /// </summary>
        public event EventHandler<KeyEventArgs>? KeyDown;

        /// <summary>
        /// Raised when a key is released.
        /// </summary>
        public event EventHandler<KeyEventArgs>? KeyUp;

        /// <summary>
        /// Raised when a mouse button is pressed.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseDown;

        /// <summary>
        /// Raised when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseUp;

        /// <summary>
        /// Raised when the mouse moves.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseMove;

        /// <summary>
        /// Raised when the mouse wheel scrolls.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseWheel;

        /// <summary>
        /// Raised when focus changes.
        /// </summary>
        public event EventHandler<FocusChangedEventArgs>? FocusChanged;

        private InputManager() { }

        /// <summary>
        /// Attaches the input manager to a form to receive input events.
        /// </summary>
        public void AttachToForm(Form form)
        {
            form.KeyPreview = true;
            form.KeyDown += OnFormKeyDown;
            form.KeyUp += OnFormKeyUp;
            form.MouseDown += OnFormMouseDown;
            form.MouseUp += OnFormMouseUp;
            form.MouseMove += OnFormMouseMove;
            form.MouseWheel += OnFormMouseWheel;
        }

        /// <summary>
        /// Detaches the input manager from a form.
        /// </summary>
        public void DetachFromForm(Form form)
        {
            form.KeyDown -= OnFormKeyDown;
            form.KeyUp -= OnFormKeyUp;
            form.MouseDown -= OnFormMouseDown;
            form.MouseUp -= OnFormMouseUp;
            form.MouseMove -= OnFormMouseMove;
            form.MouseWheel -= OnFormMouseWheel;
        }

        /// <summary>
        /// Sets the focused control.
        /// </summary>
        public void SetFocus(Control? control)
        {
            if (_focusedControl == control) return;

            var previous = _focusedControl;
            _focusedControl = control;
            FocusChanged?.Invoke(this, new FocusChangedEventArgs(previous, control));
        }

        /// <summary>
        /// Updates the hovered control based on mouse position.
        /// </summary>
        public void UpdateHoveredControl(Control? control)
        {
            _hoveredControl = control;
        }

        /// <summary>
        /// Resets the scroll delta at the end of each frame.
        /// </summary>
        public void EndFrame()
        {
            _mouseState.ResetScrollDelta();
        }

        private void OnFormKeyDown(object? sender, KeyEventArgs e)
        {
            _keyboardState.SetKeyDown(e.KeyCode);

            // Process hotkeys first
            var modifiers = GetCurrentModifiers();
            if (_hotkeyManager.ProcessKeyPress(e.KeyCode, modifiers))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            KeyDown?.Invoke(this, e);
        }

        private void OnFormKeyUp(object? sender, KeyEventArgs e)
        {
            _keyboardState.SetKeyUp(e.KeyCode);
            KeyUp?.Invoke(this, e);
        }

        private void OnFormMouseDown(object? sender, MouseEventArgs e)
        {
            _mouseState.SetButtonDown(e.Button);
            MouseDown?.Invoke(this, e);
        }

        private void OnFormMouseUp(object? sender, MouseEventArgs e)
        {
            _mouseState.SetButtonUp(e.Button);
            MouseUp?.Invoke(this, e);
        }

        private void OnFormMouseMove(object? sender, MouseEventArgs e)
        {
            _mouseState.Position = e.Location;
            MouseMove?.Invoke(this, e);
        }

        private void OnFormMouseWheel(object? sender, MouseEventArgs e)
        {
            _mouseState.SetScrollDelta(e.Delta);
            MouseWheel?.Invoke(this, e);
        }

        private KeyModifiers GetCurrentModifiers()
        {
            var modifiers = KeyModifiers.None;
            if (Control.ModifierKeys.HasFlag(Keys.Shift)) modifiers |= KeyModifiers.Shift;
            if (Control.ModifierKeys.HasFlag(Keys.Control)) modifiers |= KeyModifiers.Control;
            if (Control.ModifierKeys.HasFlag(Keys.Alt)) modifiers |= KeyModifiers.Alt;
            return modifiers;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _keyboardState.Clear();
            _mouseState.Clear();
            _hotkeyManager.Clear();
        }
    }

    /// <summary>
    /// Event args for focus changes.
    /// </summary>
    public class FocusChangedEventArgs : EventArgs
    {
        public Control? PreviousControl { get; }
        public Control? NewControl { get; }

        public FocusChangedEventArgs(Control? previous, Control? next)
        {
            PreviousControl = previous;
            NewControl = next;
        }
    }
}
