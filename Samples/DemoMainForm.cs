// File: MUIBridge/Samples/DemoMainForm.cs
// Purpose: Demo main form showing all MUIBridge controls.
// Shows how legacy WinForms code migrates to MUIBridge.

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MUIBridge.Controls;
using MUIBridge.Core;
using MUIBridge.Forms;
using MUIBridge.Prediction.Interfaces;

namespace MUIBridge.Samples
{
    /// <summary>
    /// Demo form showcasing all MUIBridge controls.
    /// This is what a migrated SMB application might look like.
    /// </summary>
    public class DemoMainForm : MUIForm
    {
        private readonly IPredictionTelemetryBus _telemetryBus;
        private readonly ILogger<DemoMainForm>? _logger;

        // Controls
        private MUIPanel _headerPanel = null!;
        private MUILabel _titleLabel = null!;
        private MUIPanel _controlsPanel = null!;
        private MUITextBox _nameTextBox = null!;
        private MUIComboBox _themeComboBox = null!;
        private MUICheckBox _moodCheckBox = null!;
        private MUIButton _submitButton = null!;
        private MUIButton _cancelButton = null!;
        private MUIProgressBar _progressBar = null!;
        private MUIDataGridView _dataGrid = null!;
        private MUILabel _statusLabel = null!;

        // Mood simulation
        private Timer _moodTimer = null!;
        private float _simulatedMood = 0f;
        private bool _moodIncreasing = true;

        public DemoMainForm(
            ThemeManager themeManager,
            IPredictionTelemetryBus telemetryBus,
            ILogger<DemoMainForm>? logger = null)
            : base(themeManager, logger)
        {
            _telemetryBus = telemetryBus;
            _logger = logger;

            InitializeComponent(themeManager);
            SetupMoodSimulation();

            _logger?.LogInformation("DemoMainForm initialized.");
        }

        private void InitializeComponent(ThemeManager themeManager)
        {
            // Form settings
            Text = "MUIBridge Demo - SMB Migration Example";
            Size = new Size(900, 700);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 600);

            // === Header Panel ===
            _headerPanel = new MUIPanel(themeManager)
            {
                Dock = DockStyle.Top,
                Height = 60,
                ShowBorder = false,
                MoodAffectsBackground = true
            };

            _titleLabel = new MUILabel(themeManager)
            {
                Text = "🎮 MUIBridge Demo - Amiga MUI Spirit Meets Modern .NET",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _headerPanel.Controls.Add(_titleLabel);

            // === Controls Panel ===
            _controlsPanel = new MUIPanel(themeManager)
            {
                Location = new Point(20, 80),
                Size = new Size(400, 280),
                ShowBorder = true
            };

            // Name Label
            var nameLabel = new MUILabel(themeManager)
            {
                Text = "Customer Name:",
                Location = new Point(15, 20),
                Size = new Size(120, 25)
            };
            _controlsPanel.Controls.Add(nameLabel);

            // Name TextBox
            _nameTextBox = new MUITextBox(themeManager)
            {
                Location = new Point(140, 20),
                Size = new Size(230, 25)
            };
            _controlsPanel.Controls.Add(_nameTextBox);

            // Theme Label
            var themeLabel = new MUILabel(themeManager)
            {
                Text = "Select Theme:",
                Location = new Point(15, 60),
                Size = new Size(120, 25)
            };
            _controlsPanel.Controls.Add(themeLabel);

            // Theme ComboBox
            _themeComboBox = new MUIComboBox(themeManager)
            {
                Location = new Point(140, 60),
                Size = new Size(230, 25)
            };
            _themeComboBox.Items.AddRange(new object[] { "Light", "Dark", "AmigaWorkbench" });
            _themeComboBox.SelectedIndex = 0;
            _themeComboBox.SelectedIndexChanged += ThemeComboBox_SelectedIndexChanged;
            _controlsPanel.Controls.Add(_themeComboBox);

            // Mood CheckBox
            _moodCheckBox = new MUICheckBox(themeManager)
            {
                Text = "Enable Mood Simulation",
                Location = new Point(15, 100),
                Size = new Size(355, 25),
                Checked = true
            };
            _moodCheckBox.CheckedChanged += MoodCheckBox_CheckedChanged;
            _controlsPanel.Controls.Add(_moodCheckBox);

            // Progress Bar
            var progressLabel = new MUILabel(themeManager)
            {
                Text = "Mood Intensity:",
                Location = new Point(15, 140),
                Size = new Size(120, 25)
            };
            _controlsPanel.Controls.Add(progressLabel);

            _progressBar = new MUIProgressBar(themeManager)
            {
                Location = new Point(140, 145),
                Size = new Size(230, 15),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            _controlsPanel.Controls.Add(_progressBar);

            // Buttons
            _submitButton = new MUIButton(themeManager)
            {
                Text = "Submit",
                Location = new Point(140, 180),
                Size = new Size(100, 35)
            };
            _submitButton.Click += SubmitButton_Click;
            _controlsPanel.Controls.Add(_submitButton);

            _cancelButton = new MUIButton(themeManager)
            {
                Text = "Cancel",
                Location = new Point(250, 180),
                Size = new Size(100, 35)
            };
            _cancelButton.Click += CancelButton_Click;
            _controlsPanel.Controls.Add(_cancelButton);

            // Info Label
            var infoLabel = new MUILabel(themeManager)
            {
                Text = "💡 Tip: Watch the UI elements react to mood changes!\n" +
                       "The prediction layer simulates AI confidence scores.",
                Location = new Point(15, 225),
                Size = new Size(370, 45),
                ForeColor = Color.Gray
            };
            _controlsPanel.Controls.Add(infoLabel);

            // === Data Grid ===
            _dataGrid = new MUIDataGridView(themeManager)
            {
                Location = new Point(440, 80),
                Size = new Size(420, 280)
            };
            SetupSampleData();

            // === Status Label ===
            _statusLabel = new MUILabel(themeManager)
            {
                Text = "Ready - MUIBridge loaded successfully",
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Add controls to form
            Controls.Add(_headerPanel);
            Controls.Add(_controlsPanel);
            Controls.Add(_dataGrid);
            Controls.Add(_statusLabel);
        }

        private void SetupSampleData()
        {
            // Create sample data for business application demo
            var table = new DataTable("Customers");
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Company", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("Revenue", typeof(decimal));

            table.Rows.Add(1, "John Smith", "Acme Corp", "Active", 125000.00m);
            table.Rows.Add(2, "Jane Doe", "TechStart Inc", "Active", 89500.50m);
            table.Rows.Add(3, "Bob Wilson", "Global Trade", "Pending", 234000.00m);
            table.Rows.Add(4, "Alice Brown", "InnovateCo", "Active", 156780.25m);
            table.Rows.Add(5, "Charlie Davis", "RetailMax", "Inactive", 45000.00m);

            _dataGrid.DataSource = table;
        }

        private void SetupMoodSimulation()
        {
            _moodTimer = new Timer { Interval = 100 };
            _moodTimer.Tick += MoodTimer_Tick;
            _moodTimer.Start();
        }

        private async void MoodTimer_Tick(object? sender, EventArgs e)
        {
            if (!_moodCheckBox.Checked) return;

            // Simulate mood oscillation
            if (_moodIncreasing)
            {
                _simulatedMood += 0.02f;
                if (_simulatedMood >= 1f)
                {
                    _simulatedMood = 1f;
                    _moodIncreasing = false;
                }
            }
            else
            {
                _simulatedMood -= 0.015f;
                if (_simulatedMood <= 0f)
                {
                    _simulatedMood = 0f;
                    _moodIncreasing = true;
                }
            }

            // Update progress bar
            _progressBar.Value = (int)(_simulatedMood * 100);

            // Publish prediction to telemetry bus
            var packet = new PredictionPacket
            {
                Confidence = _simulatedMood,
                PredictedValue = "User engagement",
                SourceModel = "DemoSimulator",
                MoodLabel = _simulatedMood switch
                {
                    >= 0.75f => "Optimal",
                    >= 0.5f => "Engaged",
                    >= 0.25f => "Neutral",
                    _ => "Low"
                }
            };

            await _telemetryBus.PublishPredictionAsync(packet);
        }

        private void ThemeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string themeName = _themeComboBox.SelectedItem?.ToString() ?? "Light";
            _themeManager?.SetTheme(themeName);
            _statusLabel.Text = $"Theme changed to: {themeName}";
            _logger?.LogInformation("Theme changed to {Theme}", themeName);
        }

        private void MoodCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (!_moodCheckBox.Checked)
            {
                _simulatedMood = 0f;
                _progressBar.Value = 0;
                _statusLabel.Text = "Mood simulation disabled";
            }
            else
            {
                _statusLabel.Text = "Mood simulation enabled";
            }
        }

        private void SubmitButton_Click(object? sender, EventArgs e)
        {
            string name = _nameTextBox.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a customer name.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _statusLabel.Text = $"Submitted: {name}";
            _logger?.LogInformation("Form submitted with name: {Name}", name);
            MessageBox.Show($"Customer '{name}' submitted successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            _nameTextBox.Text = string.Empty;
            _statusLabel.Text = "Form cleared";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _moodTimer?.Stop();
            _moodTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
