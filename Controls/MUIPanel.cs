// File: MUIBridge/Controls/MUIPanel.cs
// Purpose: WinForms-compatible Panel with mood reactivity.
// 100% drop-in replacement for System.Windows.Forms.Panel.

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
    /// Material-inspired Panel control with optional mood-reactive borders/backgrounds.
    /// Drop-in replacement for System.Windows.Forms.Panel.
    /// </summary>
    public class MUIPanel : Panel, IMoodReactiveControl
    {
        private readonly ThemeManager? _themeManager;
        private readonly ILogger<MUIPanel>? _logger;

        private Color _baseBackColor;
        private Color _baseBorderColor;
        private Color _currentBackColor;
        private float _cornerRadius;
        private float _borderThickness;
        private float _currentMoodIntensity;
        private bool _showBorder = true;
        private bool _moodAffectsBackground = false;

        /// <summary>
        /// Gets or sets whether to show the border.
        /// </summary>
        public bool ShowBorder
        {
            get => _showBorder;
            set { _showBorder = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets whether mood affects the background color.
        /// </summary>
        public bool MoodAffectsBackground
        {
            get => _moodAffectsBackground;
            set { _moodAffectsBackground = value; Invalidate(); }
        }

        /// <summary>
        /// Creates a new MUIPanel with dependency injection support.
        /// </summary>
        public MUIPanel(ThemeManager themeManager, ILogger<MUIPanel>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeControl();
            ApplyTheme(_themeManager.CurrentTheme);
            UpdateMoodVisuals(_themeManager.CurrentPredictionMoodIntensity, _themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _themeManager.PredictionMoodChanged += ThemeManager_PredictionMoodChanged;

            _logger?.LogDebug("MUIPanel created with ThemeManager.");
        }

        /// <summary>
        /// Creates a new MUIPanel without ThemeManager (for designer compatibility).
        /// </summary>
        public MUIPanel()
        {
            InitializeControl();

            _baseBackColor = Color.White;
            _baseBorderColor = Color.FromArgb(200, 200, 200);
            _currentBackColor = _baseBackColor;
            _cornerRadius = 4f;
            _borderThickness = 1f;
        }

        private void InitializeControl()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BorderStyle = BorderStyle.None;
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
        /// Applies a theme to this panel.
        /// </summary>
        public void ApplyTheme(ITheme theme)
        {
            _baseBackColor = theme.BackgroundColor;
            _baseBorderColor = theme.BorderColor;
            _currentBackColor = _baseBackColor;
            _cornerRadius = theme.CornerRadius;
            _borderThickness = theme.BorderThickness;

            Invalidate();
            _logger?.LogTrace("Theme '{Theme}' applied", theme.Name);
        }

        /// <inheritdoc />
        public void UpdateMoodVisuals(float moodIntensity, ITheme activeTheme)
        {
            _currentMoodIntensity = Math.Clamp(moodIntensity, 0f, 1f);

            if (_moodAffectsBackground && _currentMoodIntensity > 0)
            {
                Color pulseColor = activeTheme.GetCurrentPulseColor();
                _currentBackColor = WaveFXColorBlender.Lerp(_baseBackColor, pulseColor, _currentMoodIntensity * 0.2f);
            }
            else
            {
                _currentBackColor = _baseBackColor;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(
                _borderThickness / 2,
                _borderThickness / 2,
                Width - _borderThickness,
                Height - _borderThickness);

            using (var path = GetRoundedRectPath(rect, _cornerRadius))
            {
                // Background
                using (var brush = new SolidBrush(_currentBackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Border
                if (_showBorder && _borderThickness > 0)
                {
                    Color borderColor = _baseBorderColor;
                    if (_currentMoodIntensity > 0 && _themeManager != null)
                    {
                        borderColor = WaveFXColorBlender.Lerp(
                            _baseBorderColor,
                            _themeManager.CurrentTheme.AccentColor,
                            _currentMoodIntensity * 0.3f);
                    }

                    using (var pen = new Pen(borderColor, _borderThickness))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        private static GraphicsPath GetRoundedRectPath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0 || rect.Width <= 0 || rect.Height <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2;
            diameter = Math.Min(diameter, Math.Min(rect.Width, rect.Height));

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

                _logger?.LogDebug("MUIPanel disposed");
            }

            base.Dispose(disposing);
        }
    }
}
