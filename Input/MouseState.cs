// File: MUIBridge/Input/MouseState.cs
// Purpose: Thread-safe mouse state tracking (ported from DUSK).

using System.Collections.Concurrent;
using System.Drawing;

namespace MUIBridge.Input
{
    /// <summary>
    /// Thread-safe mouse state tracking.
    /// Tracks button states, position, scroll wheel, and drag operations.
    /// </summary>
    public class MouseState
    {
        private readonly ConcurrentDictionary<MouseButtons, ButtonState> _buttonStates = new();
        private Point _position;
        private int _scrollWheelValue;
        private int _scrollDelta;
        private readonly object _positionLock = new();

        /// <summary>
        /// Gets the current mouse position.
        /// </summary>
        public Point Position
        {
            get { lock (_positionLock) return _position; }
            set { lock (_positionLock) _position = value; }
        }

        /// <summary>
        /// Gets the X coordinate of the mouse.
        /// </summary>
        public int X => Position.X;

        /// <summary>
        /// Gets the Y coordinate of the mouse.
        /// </summary>
        public int Y => Position.Y;

        /// <summary>
        /// Gets the cumulative scroll wheel value.
        /// </summary>
        public int ScrollWheelValue => Interlocked.CompareExchange(ref _scrollWheelValue, 0, 0);

        /// <summary>
        /// Gets the scroll delta since last update.
        /// </summary>
        public int ScrollDelta => Interlocked.CompareExchange(ref _scrollDelta, 0, 0);

        /// <summary>
        /// Checks if a mouse button is currently pressed.
        /// </summary>
        public bool IsButtonDown(MouseButtons button) => _buttonStates.ContainsKey(button);

        /// <summary>
        /// Checks if the left mouse button is pressed.
        /// </summary>
        public bool LeftButton => IsButtonDown(MouseButtons.Left);

        /// <summary>
        /// Checks if the right mouse button is pressed.
        /// </summary>
        public bool RightButton => IsButtonDown(MouseButtons.Right);

        /// <summary>
        /// Checks if the middle mouse button is pressed.
        /// </summary>
        public bool MiddleButton => IsButtonDown(MouseButtons.Middle);

        /// <summary>
        /// Gets how long a button has been held down.
        /// </summary>
        public TimeSpan GetButtonHoldDuration(MouseButtons button)
        {
            if (_buttonStates.TryGetValue(button, out var state))
            {
                return DateTime.UtcNow - state.PressTime;
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the position where a button was pressed.
        /// </summary>
        public Point? GetButtonPressPosition(MouseButtons button)
        {
            if (_buttonStates.TryGetValue(button, out var state))
            {
                return state.PressPosition;
            }
            return null;
        }

        /// <summary>
        /// Calculates the drag delta from press position to current position.
        /// </summary>
        public Size GetDragDelta(MouseButtons button)
        {
            var pressPos = GetButtonPressPosition(button);
            if (pressPos == null) return Size.Empty;

            var current = Position;
            return new Size(current.X - pressPos.Value.X, current.Y - pressPos.Value.Y);
        }

        internal void SetButtonDown(MouseButtons button)
        {
            var pos = Position;
            _buttonStates.TryAdd(button, new ButtonState(DateTime.UtcNow, pos));
        }

        internal void SetButtonUp(MouseButtons button)
        {
            _buttonStates.TryRemove(button, out _);
        }

        internal void SetScrollDelta(int delta)
        {
            Interlocked.Exchange(ref _scrollDelta, delta);
            Interlocked.Add(ref _scrollWheelValue, delta);
        }

        internal void ResetScrollDelta()
        {
            Interlocked.Exchange(ref _scrollDelta, 0);
        }

        internal void Clear()
        {
            _buttonStates.Clear();
            ResetScrollDelta();
        }

        private readonly record struct ButtonState(DateTime PressTime, Point PressPosition);
    }
}
