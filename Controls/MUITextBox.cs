// File: MUIBridge/Controls/MUITextBox.cs
// Purpose: WinForms-compatible TextBox with mood reactivity.
// 100% drop-in replacement for System.Windows.Forms.TextBox.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MUIBridge.Core;
using MUIBridge.Utils;

namespace MUIBridge.Controls
{
    /// <summary>
    /// Material-inspired TextBox control with AI-driven mood reactivity.
    /// Drop-in replacement for System.Windows.Forms.TextBox.
    /// Features: Custom border styling, focus effects, mood-reactive borders.
    /// </summary>
    public class MUITextBox : TextBox, IMoodReactiveControl
    {
        private readonly ThemeManager? _themeManager;
        private readonly ILogger<MUITextBox>? _logger;

        private Color _baseBorderColor;
        private Color _focusBorderColor;
        private Color _currentBorderColor;
        private Color _baseBackColor;
        private float _borderThickness = 1f;
        private float _cornerRadius = 4f;
        private float _currentMoodIntensity;
        private bool _isFocused;

        // For custom border painting
        private Panel? _borderPanel;

        /// <summary>
        /// Creates a new MUITextBox with dependency injection support.
        /// </summary>
        public MUITextBox(ThemeManager themeManager, ILogger<MUITextBox>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeControl();
            ApplyTheme(_themeManager.CurrentTheme);
            UpdateMoodVisuals(_themeManager.CurrentPredictionMoodIntensity, _themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _themeManager.PredictionMoodChanged += ThemeManager_PredictionMoodChanged;

            _logger?.LogDebug("MUITextBox created with ThemeManager.");
        }

        /// <summary>
        /// Creates a new MUITextBox without ThemeManager (for designer compatibility).
        /// </summary>
        public MUITextBox()
        {
            InitializeControl();

            // Sensible defaults
            _baseBorderColor = Color.FromArgb(200, 200, 200);
            _focusBorderColor = Color.FromArgb(0, 122, 204);
            _currentBorderColor = _baseBorderColor;
            _baseBackColor = Color.White;
        }

        private void InitializeControl()
        {
            BorderStyle = BorderStyle.None;

            // We'll use a container panel for custom border drawing
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Wraps this TextBox in a bordered panel for custom styling.
        /// Call this after adding to a parent container.
        /// </summary>
        public Panel CreateBorderedContainer()
        {
            _borderPanel = new MUITextBoxBorder(this)
            {
                Size = new Size(Width + 4, Height + 4),
                Location = Location
            };

            return _borderPanel;
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ApplyTheme(e.NewTheme);
            UpdateMoodVisuals(e.NewTheme.PredictionMoodIntensity, e.NewTheme);
            _logger?.LogTrace("Theme changed to {Theme}", e.NewTheme.Name);
        }

        private void ThemeManager_PredictionMoodChanged(object? sender, float newMoodIntensity)
        {
            if (_themeManager != null)
            {
                UpdateMoodVisuals(newMoodIntensity, _themeManager.CurrentTheme);
            }
        }

        /// <summary>
        /// Applies a theme to this textbox.
        /// </summary>
        public void ApplyTheme(ITheme theme)
        {
            _baseBorderColor = theme.BorderColor;
            _focusBorderColor = theme.AccentColor;
            _baseBackColor = theme.BackgroundColor;
            _borderThickness = theme.BorderThickness;
            _cornerRadius = theme.CornerRadius;

            BackColor = _baseBackColor;
            ForeColor = theme.TextColor;

            try
            {
                Font = new Font(theme.DefaultFontName, theme.DefaultFontSize);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to apply font");
            }

            UpdateBorderColor();
            _borderPanel?.Invalidate();

            _logger?.LogTrace("Theme '{Theme}' applied", theme.Name);
        }

        /// <inheritdoc />
        public void UpdateMoodVisuals(float moodIntensity, ITheme activeTheme)
        {
            _currentMoodIntensity = Math.Clamp(moodIntensity, 0f, 1f);
            UpdateBorderColor();
            _borderPanel?.Invalidate();
        }

        private void UpdateBorderColor()
        {
            Color targetColor = _isFocused ? _focusBorderColor : _baseBorderColor;

            // Apply mood effect to border
            if (_currentMoodIntensity > 0 && _themeManager != null)
            {
                Color pulseColor = _themeManager.CurrentTheme.GetCurrentPulseColor();
                targetColor = WaveFXColorBlender.Lerp(targetColor, pulseColor, _currentMoodIntensity * 0.5f);
            }

            _currentBorderColor = targetColor;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _isFocused = true;
            UpdateBorderColor();
            _borderPanel?.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _isFocused = false;
            UpdateBorderColor();
            _borderPanel?.Invalidate();
        }

        /// <summary>
        /// Gets the current border color (for custom container painting).
        /// </summary>
        internal Color CurrentBorderColor => _currentBorderColor;

        /// <summary>
        /// Gets the border thickness (for custom container painting).
        /// </summary>
        internal float BorderThicknessValue => _borderThickness;

        /// <summary>
        /// Gets the corner radius (for custom container painting).
        /// </summary>
        internal float CornerRadiusValue => _cornerRadius;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_themeManager != null)
                {
                    _themeManager.ThemeChanged -= ThemeManager_ThemeChanged;
                    _themeManager.PredictionMoodChanged -= ThemeManager_PredictionMoodChanged;
                }

                _borderPanel?.Dispose();
                _logger?.LogDebug("MUITextBox disposed");
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Internal panel for drawing custom TextBox border.
    /// </summary>
    internal class MUITextBoxBorder : Panel
    {
        private readonly MUITextBox _textBox;

        public MUITextBoxBorder(MUITextBox textBox)
        {
            _textBox = textBox;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Color.Transparent;
            Padding = new Padding(2);

            // Add the textbox as a child
            _textBox.Location = new Point(2, 2);
            Controls.Add(_textBox);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(
                _textBox.BorderThicknessValue / 2,
                _textBox.BorderThicknessValue / 2,
                Width - _textBox.BorderThicknessValue,
                Height - _textBox.BorderThicknessValue);

            using (var path = CreateRoundedRect(rect, _textBox.CornerRadiusValue))
            {
                // Fill background
                using (var brush = new SolidBrush(_textBox.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Draw border
                using (var pen = new Pen(_textBox.CurrentBorderColor, _textBox.BorderThicknessValue))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private static GraphicsPath CreateRoundedRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2;
            var arcRect = new RectangleF(rect.Location, new SizeF(diameter, diameter));

            path.AddArc(arcRect, 180, 90);
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
