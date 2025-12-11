// File: MUIBridge/Controls/MUICheckBox.cs
// Purpose: WinForms-compatible CheckBox with mood reactivity.
// 100% drop-in replacement for System.Windows.Forms.CheckBox.

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
    /// Material-inspired CheckBox control with AI-driven mood reactivity.
    /// Drop-in replacement for System.Windows.Forms.CheckBox.
    /// </summary>
    public class MUICheckBox : CheckBox, IMoodReactiveControl
    {
        private readonly ThemeManager? _themeManager;
        private readonly ILogger<MUICheckBox>? _logger;

        private Color _baseBackColor;
        private Color _baseForeColor;
        private Color _baseBorderColor;
        private Color _checkColor;
        private Color _currentCheckColor;
        private float _cornerRadius = 3f;
        private float _borderThickness = 2f;
        private float _currentMoodIntensity;
        private const int CheckBoxSize = 18;

        /// <summary>
        /// Creates a new MUICheckBox with dependency injection support.
        /// </summary>
        public MUICheckBox(ThemeManager themeManager, ILogger<MUICheckBox>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeControl();
            ApplyTheme(_themeManager.CurrentTheme);
            UpdateMoodVisuals(_themeManager.CurrentPredictionMoodIntensity, _themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _themeManager.PredictionMoodChanged += ThemeManager_PredictionMoodChanged;

            _logger?.LogDebug("MUICheckBox created with ThemeManager.");
        }

        /// <summary>
        /// Creates a new MUICheckBox without ThemeManager (for designer compatibility).
        /// </summary>
        public MUICheckBox()
        {
            InitializeControl();

            _baseBackColor = Color.White;
            _baseForeColor = Color.Black;
            _baseBorderColor = Color.FromArgb(200, 200, 200);
            _checkColor = Color.FromArgb(0, 122, 204);
            _currentCheckColor = _checkColor;
        }

        private void InitializeControl()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            AutoSize = false;
            Height = Math.Max(Height, CheckBoxSize + 4);
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            ApplyTheme(e.NewTheme);
            UpdateMoodVisuals(e.NewTheme.PredictionMoodIntensity, e.NewTheme);
        }

        private void ThemeManager_PredictionMoodChanged(object? sender, float newMoodIntensity)
        {
            if (_themeManager != null)
            {
                UpdateMoodVisuals(newMoodIntensity, _themeManager.CurrentTheme);
            }
        }

        /// <summary>
        /// Applies a theme to this checkbox.
        /// </summary>
        public void ApplyTheme(ITheme theme)
        {
            _baseBackColor = theme.BackgroundColor;
            _baseForeColor = theme.TextColor;
            _baseBorderColor = theme.BorderColor;
            _checkColor = theme.AccentColor;
            _currentCheckColor = _checkColor;
            _cornerRadius = Math.Min(theme.CornerRadius, CheckBoxSize / 2);
            _borderThickness = theme.BorderThickness;

            try
            {
                Font = new Font(theme.DefaultFontName, theme.DefaultFontSize);
            }
            catch { }

            Invalidate();
            _logger?.LogTrace("Theme '{Theme}' applied", theme.Name);
        }

        /// <inheritdoc />
        public void UpdateMoodVisuals(float moodIntensity, ITheme activeTheme)
        {
            _currentMoodIntensity = Math.Clamp(moodIntensity, 0f, 1f);

            if (_currentMoodIntensity > 0)
            {
                Color pulseColor = activeTheme.GetCurrentPulseColor();
                _currentCheckColor = WaveFXColorBlender.Lerp(_checkColor, pulseColor, _currentMoodIntensity * 0.5f);
            }
            else
            {
                _currentCheckColor = _checkColor;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Clear background
            e.Graphics.Clear(Parent?.BackColor ?? _baseBackColor);

            // Calculate checkbox position (vertically centered)
            int checkY = (Height - CheckBoxSize) / 2;
            var checkRect = new RectangleF(0, checkY, CheckBoxSize, CheckBoxSize);

            // Draw checkbox box
            using (var path = GetRoundedRectPath(checkRect, _cornerRadius))
            {
                // Fill
                Color fillColor = Checked ? _currentCheckColor : _baseBackColor;
                using (var brush = new SolidBrush(fillColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Border
                Color borderColor = Checked ? _currentCheckColor : _baseBorderColor;
                if (_currentMoodIntensity > 0 && !Checked)
                {
                    borderColor = WaveFXColorBlender.Lerp(borderColor, _currentCheckColor, _currentMoodIntensity * 0.3f);
                }

                using (var pen = new Pen(borderColor, _borderThickness))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            // Draw checkmark if checked
            if (Checked)
            {
                DrawCheckMark(e.Graphics, checkRect);
            }

            // Draw text
            int textX = CheckBoxSize + 6;
            var textRect = new Rectangle(textX, 0, Width - textX, Height);
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                textRect,
                _baseForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            // Draw focus rectangle
            if (Focused && ShowFocusCues)
            {
                var focusRect = new Rectangle(textX - 2, 2, Width - textX, Height - 4);
                ControlPaint.DrawFocusRectangle(e.Graphics, focusRect);
            }
        }

        private void DrawCheckMark(Graphics g, RectangleF rect)
        {
            using (var pen = new Pen(Color.White, 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                float x = rect.X + rect.Width * 0.2f;
                float y = rect.Y + rect.Height * 0.5f;

                var points = new PointF[]
                {
                    new PointF(x, y),
                    new PointF(rect.X + rect.Width * 0.4f, rect.Y + rect.Height * 0.7f),
                    new PointF(rect.X + rect.Width * 0.8f, rect.Y + rect.Height * 0.3f)
                };

                g.DrawLines(pen, points);
            }
        }

        private static GraphicsPath GetRoundedRectPath(RectangleF rect, float radius)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_themeManager != null)
                {
                    _themeManager.ThemeChanged -= ThemeManager_ThemeChanged;
                    _themeManager.PredictionMoodChanged -= ThemeManager_PredictionMoodChanged;
                }

                _logger?.LogDebug("MUICheckBox disposed");
            }

            base.Dispose(disposing);
        }
    }
}
