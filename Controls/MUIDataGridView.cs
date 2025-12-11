// File: MUIBridge/Controls/MUIDataGridView.cs
// Purpose: WinForms-compatible DataGridView with mood reactivity.
// 100% drop-in replacement for System.Windows.Forms.DataGridView.
// Critical for SMB business applications with tabular data.

using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MUIBridge.Core;
using MUIBridge.Utils;

namespace MUIBridge.Controls
{
    /// <summary>
    /// Material-inspired DataGridView control with AI-driven mood reactivity.
    /// Drop-in replacement for System.Windows.Forms.DataGridView.
    /// Perfect for business applications with tabular data.
    /// </summary>
    public class MUIDataGridView : DataGridView, IMoodReactiveControl
    {
        private readonly ThemeManager? _themeManager;
        private readonly ILogger<MUIDataGridView>? _logger;

        private Color _headerBackColor;
        private Color _headerForeColor;
        private Color _cellBackColor;
        private Color _cellForeColor;
        private Color _alternatingRowColor;
        private Color _selectionBackColor;
        private Color _selectionForeColor;
        private Color _gridLineColor;
        private float _currentMoodIntensity;

        /// <summary>
        /// Gets or sets whether to use alternating row colors.
        /// </summary>
        public bool UseAlternatingRowColors { get; set; } = true;

        /// <summary>
        /// Creates a new MUIDataGridView with dependency injection support.
        /// </summary>
        public MUIDataGridView(ThemeManager themeManager, ILogger<MUIDataGridView>? logger = null)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            InitializeControl();
            ApplyTheme(_themeManager.CurrentTheme);
            UpdateMoodVisuals(_themeManager.CurrentPredictionMoodIntensity, _themeManager.CurrentTheme);

            _themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            _themeManager.PredictionMoodChanged += ThemeManager_PredictionMoodChanged;

            _logger?.LogDebug("MUIDataGridView created with ThemeManager.");
        }

        /// <summary>
        /// Creates a new MUIDataGridView without ThemeManager (for designer compatibility).
        /// </summary>
        public MUIDataGridView()
        {
            InitializeControl();

            // Default Material-like colors
            _headerBackColor = Color.FromArgb(0, 122, 204);
            _headerForeColor = Color.White;
            _cellBackColor = Color.White;
            _cellForeColor = Color.Black;
            _alternatingRowColor = Color.FromArgb(245, 245, 245);
            _selectionBackColor = Color.FromArgb(0, 122, 204);
            _selectionForeColor = Color.White;
            _gridLineColor = Color.FromArgb(224, 224, 224);

            ApplyDefaultStyles();
        }

        private void InitializeControl()
        {
            DoubleBuffered = true;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            EnableHeadersVisualStyles = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            AllowUserToAddRows = false;
            AllowUserToResizeRows = false;
            RowHeadersVisible = false;
        }

        private void ApplyDefaultStyles()
        {
            // Header style
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _headerBackColor,
                ForeColor = _headerForeColor,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 4, 8, 4)
            };

            ColumnHeadersHeight = 40;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Cell style
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _cellBackColor,
                ForeColor = _cellForeColor,
                Font = new Font("Segoe UI", 9f),
                SelectionBackColor = _selectionBackColor,
                SelectionForeColor = _selectionForeColor,
                Padding = new Padding(8, 4, 8, 4)
            };

            // Alternating row style
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _alternatingRowColor,
                ForeColor = _cellForeColor,
                SelectionBackColor = _selectionBackColor,
                SelectionForeColor = _selectionForeColor
            };

            RowTemplate.Height = 36;
            GridColor = _gridLineColor;
            BackgroundColor = _cellBackColor;
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
        /// Applies a theme to this data grid.
        /// </summary>
        public void ApplyTheme(ITheme theme)
        {
            _headerBackColor = theme.PrimaryColor;
            _headerForeColor = Color.White; // Usually white on primary
            _cellBackColor = theme.BackgroundColor;
            _cellForeColor = theme.TextColor;
            _selectionBackColor = theme.AccentColor;
            _selectionForeColor = Color.White;
            _gridLineColor = theme.BorderColor;

            // Calculate alternating row color (slightly different from background)
            int r = Math.Max(0, _cellBackColor.R - 10);
            int g = Math.Max(0, _cellBackColor.G - 10);
            int b = Math.Max(0, _cellBackColor.B - 10);
            _alternatingRowColor = Color.FromArgb(r, g, b);

            // Apply to styles
            ColumnHeadersDefaultCellStyle.BackColor = _headerBackColor;
            ColumnHeadersDefaultCellStyle.ForeColor = _headerForeColor;

            try
            {
                ColumnHeadersDefaultCellStyle.Font = new Font(theme.DefaultFontName, theme.DefaultFontSize, FontStyle.Bold);
                DefaultCellStyle.Font = new Font(theme.DefaultFontName, theme.DefaultFontSize);
            }
            catch { }

            DefaultCellStyle.BackColor = _cellBackColor;
            DefaultCellStyle.ForeColor = _cellForeColor;
            DefaultCellStyle.SelectionBackColor = _selectionBackColor;
            DefaultCellStyle.SelectionForeColor = _selectionForeColor;

            if (UseAlternatingRowColors)
            {
                AlternatingRowsDefaultCellStyle.BackColor = _alternatingRowColor;
                AlternatingRowsDefaultCellStyle.ForeColor = _cellForeColor;
                AlternatingRowsDefaultCellStyle.SelectionBackColor = _selectionBackColor;
                AlternatingRowsDefaultCellStyle.SelectionForeColor = _selectionForeColor;
            }

            GridColor = _gridLineColor;
            BackgroundColor = _cellBackColor;

            Invalidate();
            _logger?.LogTrace("Theme '{Theme}' applied", theme.Name);
        }

        /// <inheritdoc />
        public void UpdateMoodVisuals(float moodIntensity, ITheme activeTheme)
        {
            _currentMoodIntensity = Math.Clamp(moodIntensity, 0f, 1f);

            if (_currentMoodIntensity > 0)
            {
                // Apply mood to selection color
                Color pulseColor = activeTheme.GetCurrentPulseColor();
                Color moodSelection = WaveFXColorBlender.Lerp(_selectionBackColor, pulseColor, _currentMoodIntensity * 0.3f);

                DefaultCellStyle.SelectionBackColor = moodSelection;
                AlternatingRowsDefaultCellStyle.SelectionBackColor = moodSelection;

                // Subtle grid line effect
                GridColor = WaveFXColorBlender.Lerp(_gridLineColor, pulseColor, _currentMoodIntensity * 0.2f);
            }
            else
            {
                DefaultCellStyle.SelectionBackColor = _selectionBackColor;
                AlternatingRowsDefaultCellStyle.SelectionBackColor = _selectionBackColor;
                GridColor = _gridLineColor;
            }

            Invalidate();
        }

        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            base.OnCellFormatting(e);

            // Apply mood-based row highlighting for selected rows
            if (_currentMoodIntensity > 0.5f && Rows[e.RowIndex].Selected)
            {
                // High mood = emphasize selection more
                e.CellStyle.SelectionBackColor = WaveFXColorBlender.AdjustBrightness(
                    _selectionBackColor,
                    1f + (_currentMoodIntensity * 0.2f));
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

                _logger?.LogDebug("MUIDataGridView disposed");
            }

            base.Dispose(disposing);
        }
    }
}
