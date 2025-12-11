// File: MUIBridge/Controls/MUIComboBox.cs
// Purpose: WinForms-compatible ComboBox with mood reactivity.
// 100% drop-in replacement for System.Windows.Forms.ComboBox.

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
    /// Material-inspired ComboBox control with AI-driven mood reactivity.
    /// Drop-in replacement for System.Windows.Forms.ComboBox.
    /// </summary>
    public class MUIComboBox : ComboBox, IMoodReactiveControl
    {
        private readonly ThemeManager? _themeManager;
        private readonly ILogger<MUIComboBox>? _logger;

        private Color _baseBackColor;
        private Color _baseForeColor;
        private Color _baseBorderColor;
        private Color _currentBorderColor;
        private Color _dropdownArrowColor;
        private float _cornerRadius;
        private float _borderThickness;
        private float _currentMoodIntensity;

        /// <summary>
        /// Creates a new MUIComboBox with dependency injection support.
        /// </summary>
        public MUIComboBox(ThemeManager themeManager, ILogger<MUIComboBox>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeControl();
            ApplyTheme(_themeManager.CurrentTheme);
            UpdateMoodVisuals(_themeManager.CurrentPredictionMoodIntensity, _themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _themeManager.PredictionMoodChanged += ThemeManager_PredictionMoodChanged;

            _logger?.LogDebug("MUIComboBox created with ThemeManager.");
        }

        /// <summary>
        /// Creates a new MUIComboBox without ThemeManager (for designer compatibility).
        /// </summary>
        public MUIComboBox()
        {
            InitializeControl();

            _baseBackColor = Color.White;
            _baseForeColor = Color.Black;
            _baseBorderColor = Color.FromArgb(200, 200, 200);
            _currentBorderColor = _baseBorderColor;
            _dropdownArrowColor = Color.FromArgb(100, 100, 100);
            _cornerRadius = 4f;
            _borderThickness = 1f;
        }

        private void InitializeControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
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
        /// Applies a theme to this combobox.
        /// </summary>
        public void ApplyTheme(ITheme theme)
        {
            _baseBackColor = theme.BackgroundColor;
            _baseForeColor = theme.TextColor;
            _baseBorderColor = theme.BorderColor;
            _currentBorderColor = _baseBorderColor;
            _dropdownArrowColor = theme.TextColor;
            _cornerRadius = theme.CornerRadius;
            _borderThickness = theme.BorderThickness;

            BackColor = _baseBackColor;
            ForeColor = _baseForeColor;

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
                _currentBorderColor = WaveFXColorBlender.Lerp(_baseBorderColor, pulseColor, _currentMoodIntensity * 0.5f);
            }
            else
            {
                _currentBorderColor = _baseBorderColor;
            }

            Invalidate();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            // Determine colors based on state
            Color backColor = (e.State & DrawItemState.Selected) != 0
                ? (_themeManager?.CurrentTheme.AccentColor ?? Color.FromArgb(0, 122, 204))
                : _baseBackColor;

            Color foreColor = (e.State & DrawItemState.Selected) != 0
                ? Color.White
                : _baseForeColor;

            // Fill background
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Draw text
            string text = Items[e.Index]?.ToString() ?? string.Empty;
            TextRenderer.DrawText(
                e.Graphics,
                text,
                Font,
                e.Bounds,
                foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            e.DrawFocusRectangle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // WM_PAINT - add custom border
            if (m.Msg == 0x000F)
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    // Draw border
                    var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                    using (var pen = new Pen(_currentBorderColor, _borderThickness))
                    {
                        g.DrawRectangle(pen, rect);
                    }

                    // Draw dropdown arrow
                    int arrowSize = 8;
                    int arrowX = Width - arrowSize - 8;
                    int arrowY = (Height - arrowSize / 2) / 2;

                    var arrowPoints = new Point[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX + arrowSize, arrowY),
                        new Point(arrowX + arrowSize / 2, arrowY + arrowSize / 2)
                    };

                    using (var brush = new SolidBrush(_dropdownArrowColor))
                    {
                        g.FillPolygon(brush, arrowPoints);
                    }
                }
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _currentBorderColor = _themeManager?.CurrentTheme.AccentColor ?? Color.FromArgb(0, 122, 204);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _currentBorderColor = _baseBorderColor;
            if (_currentMoodIntensity > 0 && _themeManager != null)
            {
                Color pulseColor = _themeManager.CurrentTheme.GetCurrentPulseColor();
                _currentBorderColor = WaveFXColorBlender.Lerp(_baseBorderColor, pulseColor, _currentMoodIntensity * 0.5f);
            }
            Invalidate();
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

                _logger?.LogDebug("MUIComboBox disposed");
            }

            base.Dispose(disposing);
        }
    }
}
