// File: MUIBridge/Studio/LiveThemeEditor.cs
// Purpose: MUI-style live theme editor with RGB/HSV sliders (ported from DUSK).

using System.Drawing;
using System.Windows.Forms;
using MUIBridge.Core;

namespace MUIBridge.Studio
{
    /// <summary>
    /// Live theme editor with clickable color swatches and RGB/HSV sliders.
    /// Provides immediate visual feedback for theme customization.
    /// </summary>
    public class LiveThemeEditor : UserControl
    {
        private readonly ThemeManager _themeManager;
        private readonly Panel _swatchPanel;
        private readonly Panel _sliderPanel;
        private readonly Panel _previewPanel;

        private ColorTarget _selectedTarget = ColorTarget.Primary;
        private readonly Dictionary<ColorTarget, Color> _workingColors = new();
        private readonly Dictionary<ColorTarget, Button> _swatchButtons = new();

        // Sliders
        private TrackBar? _redSlider;
        private TrackBar? _greenSlider;
        private TrackBar? _blueSlider;
        private Label? _colorPreview;

        /// <summary>
        /// Raised when a color is changed in the editor.
        /// </summary>
        public event EventHandler<ThemeColorChangedEventArgs>? ColorChanged;

        public LiveThemeEditor(ThemeManager themeManager)
        {
            _themeManager = themeManager;

            Size = new Size(400, 350);
            BackColor = Color.FromArgb(45, 45, 48);

            // Initialize working colors from current theme
            InitializeWorkingColors();

            // Create panels
            _swatchPanel = CreateSwatchPanel();
            _sliderPanel = CreateSliderPanel();
            _previewPanel = CreatePreviewPanel();

            Controls.Add(_swatchPanel);
            Controls.Add(_sliderPanel);
            Controls.Add(_previewPanel);
        }

        private void InitializeWorkingColors()
        {
            var theme = _themeManager.CurrentTheme;
            _workingColors[ColorTarget.Primary] = theme.PrimaryColor;
            _workingColors[ColorTarget.Secondary] = theme.SecondaryColor;
            _workingColors[ColorTarget.Background] = theme.BackgroundColor;
            _workingColors[ColorTarget.Text] = theme.TextColor;
            _workingColors[ColorTarget.Border] = theme.BorderColor;
            _workingColors[ColorTarget.Accent] = theme.AccentColor;
        }

        private Panel CreateSwatchPanel()
        {
            var panel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(380, 80),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var targets = Enum.GetValues<ColorTarget>();
            int x = 10;

            foreach (var target in targets)
            {
                var swatch = new Button
                {
                    Location = new Point(x, 10),
                    Size = new Size(50, 50),
                    BackColor = _workingColors.GetValueOrDefault(target, Color.Gray),
                    FlatStyle = FlatStyle.Flat,
                    Tag = target
                };
                swatch.FlatAppearance.BorderSize = _selectedTarget == target ? 3 : 1;
                swatch.FlatAppearance.BorderColor = _selectedTarget == target ? Color.White : Color.DarkGray;
                swatch.Click += OnSwatchClick;

                var label = new Label
                {
                    Location = new Point(x, 62),
                    Size = new Size(50, 15),
                    Text = target.ToString()[..3],
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 7),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                panel.Controls.Add(swatch);
                panel.Controls.Add(label);
                _swatchButtons[target] = swatch;
                x += 60;
            }

            return panel;
        }

        private Panel CreateSliderPanel()
        {
            var panel = new Panel
            {
                Location = new Point(10, 100),
                Size = new Size(380, 120),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            // Red slider
            var redLabel = new Label { Location = new Point(10, 10), Text = "R", ForeColor = Color.Red, Size = new Size(20, 20) };
            _redSlider = CreateSlider(40, 10, Color.Red);
            _redSlider.ValueChanged += OnSliderChanged;

            // Green slider
            var greenLabel = new Label { Location = new Point(10, 45), Text = "G", ForeColor = Color.LimeGreen, Size = new Size(20, 20) };
            _greenSlider = CreateSlider(40, 45, Color.LimeGreen);
            _greenSlider.ValueChanged += OnSliderChanged;

            // Blue slider
            var blueLabel = new Label { Location = new Point(10, 80), Text = "B", ForeColor = Color.DodgerBlue, Size = new Size(20, 20) };
            _blueSlider = CreateSlider(40, 80, Color.DodgerBlue);
            _blueSlider.ValueChanged += OnSliderChanged;

            // Color preview
            _colorPreview = new Label
            {
                Location = new Point(310, 30),
                Size = new Size(60, 60),
                BackColor = _workingColors[_selectedTarget]
            };

            panel.Controls.AddRange(new Control[] { redLabel, _redSlider, greenLabel, _greenSlider, blueLabel, _blueSlider, _colorPreview });

            UpdateSlidersFromColor(_workingColors[_selectedTarget]);

            return panel;
        }

        private TrackBar CreateSlider(int x, int y, Color tickColor)
        {
            return new TrackBar
            {
                Location = new Point(x, y),
                Size = new Size(260, 30),
                Minimum = 0,
                Maximum = 255,
                TickFrequency = 32,
                BackColor = Color.FromArgb(30, 30, 30)
            };
        }

        private Panel CreatePreviewPanel()
        {
            var panel = new Panel
            {
                Location = new Point(10, 230),
                Size = new Size(380, 110),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var presetLabel = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(360, 20),
                Text = "Presets:",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var presets = new[] { "MUI Classic", "MUI Royale", "Modern Flat", "Dark Mode", "Amiga" };
            int x = 10;

            panel.Controls.Add(presetLabel);

            foreach (var preset in presets)
            {
                var btn = new Button
                {
                    Location = new Point(x, 35),
                    Size = new Size(70, 25),
                    Text = preset,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 7),
                    Tag = preset
                };
                btn.FlatAppearance.BorderColor = Color.Gray;
                btn.Click += OnPresetClick;
                panel.Controls.Add(btn);
                x += 75;
            }

            // Apply button
            var applyBtn = new Button
            {
                Location = new Point(280, 75),
                Size = new Size(90, 30),
                Text = "Apply Theme",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            applyBtn.Click += (s, e) => ApplyTheme();
            panel.Controls.Add(applyBtn);

            return panel;
        }

        private void OnSwatchClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is ColorTarget target)
            {
                // Update selection
                foreach (var kvp in _swatchButtons)
                {
                    kvp.Value.FlatAppearance.BorderSize = 1;
                    kvp.Value.FlatAppearance.BorderColor = Color.DarkGray;
                }

                btn.FlatAppearance.BorderSize = 3;
                btn.FlatAppearance.BorderColor = Color.White;
                _selectedTarget = target;

                UpdateSlidersFromColor(_workingColors[target]);
            }
        }

        private void OnSliderChanged(object? sender, EventArgs e)
        {
            if (_redSlider == null || _greenSlider == null || _blueSlider == null) return;

            var newColor = Color.FromArgb(_redSlider.Value, _greenSlider.Value, _blueSlider.Value);
            _workingColors[_selectedTarget] = newColor;

            if (_swatchButtons.TryGetValue(_selectedTarget, out var swatch))
            {
                swatch.BackColor = newColor;
            }

            if (_colorPreview != null)
            {
                _colorPreview.BackColor = newColor;
            }

            ColorChanged?.Invoke(this, new ThemeColorChangedEventArgs(_selectedTarget, newColor));
        }

        private void OnPresetClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string preset)
            {
                ApplyPreset(preset);
            }
        }

        private void ApplyPreset(string presetName)
        {
            switch (presetName)
            {
                case "MUI Classic":
                    _workingColors[ColorTarget.Primary] = Color.FromArgb(170, 170, 170);
                    _workingColors[ColorTarget.Secondary] = Color.FromArgb(136, 136, 136);
                    _workingColors[ColorTarget.Background] = Color.FromArgb(170, 170, 170);
                    _workingColors[ColorTarget.Text] = Color.Black;
                    _workingColors[ColorTarget.Border] = Color.FromArgb(0, 0, 0);
                    _workingColors[ColorTarget.Accent] = Color.FromArgb(120, 170, 230);
                    break;

                case "MUI Royale":
                    _workingColors[ColorTarget.Primary] = Color.FromArgb(100, 100, 180);
                    _workingColors[ColorTarget.Secondary] = Color.FromArgb(80, 80, 160);
                    _workingColors[ColorTarget.Background] = Color.FromArgb(50, 50, 90);
                    _workingColors[ColorTarget.Text] = Color.White;
                    _workingColors[ColorTarget.Border] = Color.FromArgb(120, 120, 200);
                    _workingColors[ColorTarget.Accent] = Color.FromArgb(255, 200, 100);
                    break;

                case "Modern Flat":
                    _workingColors[ColorTarget.Primary] = Color.FromArgb(0, 122, 204);
                    _workingColors[ColorTarget.Secondary] = Color.FromArgb(45, 45, 48);
                    _workingColors[ColorTarget.Background] = Color.FromArgb(30, 30, 30);
                    _workingColors[ColorTarget.Text] = Color.White;
                    _workingColors[ColorTarget.Border] = Color.FromArgb(63, 63, 70);
                    _workingColors[ColorTarget.Accent] = Color.FromArgb(0, 200, 83);
                    break;

                case "Dark Mode":
                    _workingColors[ColorTarget.Primary] = Color.FromArgb(60, 60, 60);
                    _workingColors[ColorTarget.Secondary] = Color.FromArgb(45, 45, 45);
                    _workingColors[ColorTarget.Background] = Color.FromArgb(25, 25, 25);
                    _workingColors[ColorTarget.Text] = Color.FromArgb(220, 220, 220);
                    _workingColors[ColorTarget.Border] = Color.FromArgb(80, 80, 80);
                    _workingColors[ColorTarget.Accent] = Color.FromArgb(0, 150, 136);
                    break;

                case "Amiga":
                    _workingColors[ColorTarget.Primary] = Color.FromArgb(102, 136, 187);
                    _workingColors[ColorTarget.Secondary] = Color.FromArgb(170, 170, 170);
                    _workingColors[ColorTarget.Background] = Color.FromArgb(102, 136, 187);
                    _workingColors[ColorTarget.Text] = Color.Black;
                    _workingColors[ColorTarget.Border] = Color.White;
                    _workingColors[ColorTarget.Accent] = Color.FromArgb(255, 136, 0);
                    break;
            }

            // Update all swatches
            foreach (var kvp in _swatchButtons)
            {
                if (_workingColors.TryGetValue(kvp.Key, out var color))
                {
                    kvp.Value.BackColor = color;
                }
            }

            UpdateSlidersFromColor(_workingColors[_selectedTarget]);
        }

        private void UpdateSlidersFromColor(Color color)
        {
            if (_redSlider != null) _redSlider.Value = color.R;
            if (_greenSlider != null) _greenSlider.Value = color.G;
            if (_blueSlider != null) _blueSlider.Value = color.B;
            if (_colorPreview != null) _colorPreview.BackColor = color;
        }

        private void ApplyTheme()
        {
            // Notify about all color changes
            foreach (var kvp in _workingColors)
            {
                ColorChanged?.Invoke(this, new ThemeColorChangedEventArgs(kvp.Key, kvp.Value));
            }
        }
    }

    /// <summary>
    /// Target colors that can be edited.
    /// </summary>
    public enum ColorTarget
    {
        Primary,
        Secondary,
        Background,
        Text,
        Border,
        Accent
    }

    /// <summary>
    /// Event args for theme color changes.
    /// </summary>
    public class ThemeColorChangedEventArgs : EventArgs
    {
        public ColorTarget Target { get; }
        public Color NewColor { get; }

        public ThemeColorChangedEventArgs(ColorTarget target, Color newColor)
        {
            Target = target;
            NewColor = newColor;
        }
    }
}
