// File: MUIBridge/Controls/MUIProgressBar.cs
// Purpose: WinForms-compatible ProgressBar with mood reactivity.
// 100% drop-in replacement for System.Windows.Forms.ProgressBar.

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
    /// Material-inspired ProgressBar control with AI-driven mood reactivity.
    /// Drop-in replacement for System.Windows.Forms.ProgressBar.
    /// The progress color shifts based on mood intensity.
    /// </summary>
    public class MUIProgressBar : ProgressBar, IMoodReactiveControl
    {
        private readonly ThemeManager? _themeManager;
        private readonly ILogger<MUIProgressBar>? _logger;

        private Color _baseBackColor;
        private Color _progressColor;
        private Color _currentProgressColor;
        private float _cornerRadius;
        private float _currentMoodIntensity;

        /// <summary>
        /// Creates a new MUIProgressBar with dependency injection support.
        /// </summary>
        public MUIProgressBar(ThemeManager themeManager, ILogger<MUIProgressBar>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeControl();
            ApplyTheme(_themeManager.CurrentTheme);
            UpdateMoodVisuals(_themeManager.CurrentPredictionMoodIntensity, _themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _themeManager.PredictionMoodChanged += ThemeManager_PredictionMoodChanged;

            _logger?.LogDebug("MUIProgressBar created with ThemeManager.");
        }

        /// <summary>
        /// Creates a new MUIProgressBar without ThemeManager (for designer compatibility).
        /// </summary>
        public MUIProgressBar()
        {
            InitializeControl();

            _baseBackColor = Color.FromArgb(224, 224, 224);
            _progressColor = Color.FromArgb(0, 122, 204);
            _currentProgressColor = _progressColor;
            _cornerRadius = 4f;
        }

        private void InitializeControl()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            Height = 8; // Thin material-style progress bar
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
        /// Applies a theme to this progress bar.
        /// </summary>
        public void ApplyTheme(ITheme theme)
        {
            _baseBackColor = theme.SecondaryColor;
            _progressColor = theme.PrimaryColor;
            _currentProgressColor = _progressColor;
            _cornerRadius = theme.CornerRadius;

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
                _currentProgressColor = WaveFXColorBlender.Lerp(_progressColor, pulseColor, _currentMoodIntensity * 0.6f);
            }
            else
            {
                _currentProgressColor = _progressColor;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0, 0, Width, Height);

            // Draw background track
            using (var path = GetRoundedRectPath(rect, _cornerRadius))
            {
                using (var brush = new SolidBrush(_baseBackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            // Calculate progress width
            float progressPercent = (float)(Value - Minimum) / (Maximum - Minimum);
            float progressWidth = rect.Width * progressPercent;

            if (progressWidth > 0)
            {
                var progressRect = new RectangleF(0, 0, progressWidth, Height);

                using (var path = GetRoundedRectPath(progressRect, _cornerRadius))
                {
                    // Gradient effect based on mood
                    if (_currentMoodIntensity > 0.3f)
                    {
                        using (var brush = new LinearGradientBrush(
                            progressRect,
                            _currentProgressColor,
                            WaveFXColorBlender.AdjustBrightness(_currentProgressColor, 1.2f),
                            LinearGradientMode.Horizontal))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }
                    else
                    {
                        using (var brush = new SolidBrush(_currentProgressColor))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
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

            float diameter = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
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

                _logger?.LogDebug("MUIProgressBar disposed");
            }

            base.Dispose(disposing);
        }
    }
}
