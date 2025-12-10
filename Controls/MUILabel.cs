// File: MUIBridge/Controls/MUILabel.cs
// Purpose: WinForms-compatible Label with mood reactivity.
// 100% drop-in replacement for System.Windows.Forms.Label.

using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MUIBridge.Core;
using MUIBridge.Utils;

namespace MUIBridge.Controls
{
    /// <summary>
    /// Material-inspired Label control with AI-driven mood reactivity.
    /// Drop-in replacement for System.Windows.Forms.Label.
    /// </summary>
    public class MUILabel : Label, IMoodReactiveControl
    {
        private readonly ThemeManager? _themeManager;
        private readonly ILogger<MUILabel>? _logger;

        private Font? _appliedFont;
        private Color _baseForeColor;
        private Color _currentForeColor;

        /// <summary>
        /// Creates a new MUILabel with dependency injection support.
        /// </summary>
        public MUILabel(ThemeManager themeManager, ILogger<MUILabel>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeControl();
            ApplyTheme(_themeManager.CurrentTheme);
            UpdateMoodVisuals(_themeManager.CurrentPredictionMoodIntensity, _themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _themeManager.PredictionMoodChanged += ThemeManager_PredictionMoodChanged;

            _logger?.LogDebug("MUILabel created with ThemeManager.");
        }

        /// <summary>
        /// Creates a new MUILabel without ThemeManager (for designer compatibility).
        /// </summary>
        public MUILabel()
        {
            InitializeControl();

            // Sensible defaults
            _baseForeColor = Color.Black;
            _currentForeColor = _baseForeColor;
        }

        private void InitializeControl()
        {
            AutoSize = false;
            TextAlign = ContentAlignment.MiddleLeft;
            BackColor = Color.Transparent;
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
        /// Applies a theme to this label.
        /// </summary>
        public void ApplyTheme(ITheme theme)
        {
            try
            {
                _baseForeColor = theme.TextColor;
                _currentForeColor = _baseForeColor;
                ForeColor = _currentForeColor;

                var newFont = new Font(theme.DefaultFontName, theme.DefaultFontSize);
                if (Font == null || !newFont.Name.Equals(Font.Name) ||
                    Math.Abs(newFont.Size - Font.Size) > 0.1f)
                {
                    _appliedFont?.Dispose();
                    _appliedFont = newFont;
                    Font = _appliedFont;
                }
                else
                {
                    newFont.Dispose();
                }

                _logger?.LogTrace("Theme '{Theme}' applied", theme.Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to apply theme '{Theme}'", theme.Name);
                ForeColor = SystemColors.ControlText;
                Font = SystemFonts.DefaultFont;
            }

            Invalidate();
        }

        /// <inheritdoc />
        public void UpdateMoodVisuals(float moodIntensity, ITheme activeTheme)
        {
            float clamped = Math.Clamp(moodIntensity, 0f, 1f);

            // Blend text color toward pulse color based on mood
            Color targetColor;
            try
            {
                var pulseColor = activeTheme.GetCurrentPulseColor();
                targetColor = WaveFXColorBlender.Lerp(_baseForeColor, pulseColor, clamped * 0.7f);
            }
            catch
            {
                targetColor = WaveFXColorBlender.Lerp(
                    _baseForeColor,
                    activeTheme.AccentColor,
                    clamped * 0.5f);
            }

            if (ForeColor != targetColor)
            {
                _currentForeColor = targetColor;
                ForeColor = _currentForeColor;
            }
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

                _appliedFont?.Dispose();
                _appliedFont = null;

                _logger?.LogDebug("MUILabel disposed");
            }

            base.Dispose(disposing);
        }
    }
}
