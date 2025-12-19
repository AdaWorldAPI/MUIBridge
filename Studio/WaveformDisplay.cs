// File: MUIBridge/Studio/WaveformDisplay.cs
// Purpose: Winamp-style waveform display for cache "breathing" visualization (ported from DUSK).

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MUIBridge.Cache;

namespace MUIBridge.Studio
{
    /// <summary>
    /// Winamp-style waveform display control for visualizing cache activity.
    /// Shows L1/L2/L3 cache "breathing" with classic oscilloscope aesthetics.
    /// </summary>
    public class WaveformDisplay : UserControl
    {
        private readonly DataPulseMonitor? _monitor;
        private readonly Timer _refreshTimer;

        private readonly Color _l1Color = Color.FromArgb(0, 255, 128);   // Green - Memory
        private readonly Color _l2Color = Color.FromArgb(255, 128, 0);   // Orange - Redis
        private readonly Color _l3Color = Color.FromArgb(128, 128, 255); // Blue - MongoDB
        private readonly Color _backgroundColor = Color.FromArgb(10, 10, 20);
        private readonly Color _gridColor = Color.FromArgb(30, 40, 50);

        private bool _showL1 = true;
        private bool _showL2 = true;
        private bool _showL3 = true;

        /// <summary>
        /// Whether to show L1 (Memory) waveform.
        /// </summary>
        public bool ShowL1 { get => _showL1; set { _showL1 = value; Invalidate(); } }

        /// <summary>
        /// Whether to show L2 (Redis) waveform.
        /// </summary>
        public bool ShowL2 { get => _showL2; set { _showL2 = value; Invalidate(); } }

        /// <summary>
        /// Whether to show L3 (MongoDB) waveform.
        /// </summary>
        public bool ShowL3 { get => _showL3; set { _showL3 = value; Invalidate(); } }

        public WaveformDisplay(DataPulseMonitor? monitor = null)
        {
            _monitor = monitor;

            DoubleBuffered = true;
            Size = new Size(400, 150);
            BackColor = _backgroundColor;

            _refreshTimer = new Timer { Interval = 33 }; // ~30 FPS
            _refreshTimer.Tick += (s, e) => Invalidate();
            _refreshTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background
            g.Clear(_backgroundColor);

            // Draw grid
            DrawGrid(g);

            // Draw waveforms
            if (_monitor != null)
            {
                if (_showL1) DrawWaveform(g, _monitor.GetOrderedWaveform(1), _l1Color, 0);
                if (_showL2) DrawWaveform(g, _monitor.GetOrderedWaveform(2), _l2Color, 1);
                if (_showL3) DrawWaveform(g, _monitor.GetOrderedWaveform(3), _l3Color, 2);
            }
            else
            {
                // Demo mode with sine waves
                DrawDemoWaveforms(g);
            }

            // Draw legend
            DrawLegend(g);

            // Draw stats
            DrawStats(g);
        }

        private void DrawGrid(Graphics g)
        {
            using var pen = new Pen(_gridColor, 1);
            pen.DashStyle = DashStyle.Dot;

            // Horizontal lines
            for (int y = 0; y < Height; y += 25)
            {
                g.DrawLine(pen, 0, y, Width, y);
            }

            // Vertical lines
            for (int x = 0; x < Width; x += 50)
            {
                g.DrawLine(pen, x, 0, x, Height);
            }

            // Center line
            using var centerPen = new Pen(Color.FromArgb(60, 80, 100), 1);
            g.DrawLine(centerPen, 0, Height / 2, Width, Height / 2);
        }

        private void DrawWaveform(Graphics g, float[] data, Color color, int offset)
        {
            if (data.Length < 2) return;

            using var pen = new Pen(color, 2);
            using var glowPen = new Pen(Color.FromArgb(80, color), 4);

            var points = new PointF[data.Length];
            float xScale = (float)Width / data.Length;
            float yCenter = Height / 2f;
            float yScale = Height * 0.4f;

            for (int i = 0; i < data.Length; i++)
            {
                float x = i * xScale;
                float y = yCenter - (data[i] * yScale) + (offset * 5); // Slight offset for visibility
                points[i] = new PointF(x, y);
            }

            // Draw glow effect
            g.DrawLines(glowPen, points);

            // Draw main line
            g.DrawLines(pen, points);
        }

        private void DrawDemoWaveforms(Graphics g)
        {
            var time = DateTime.Now.Ticks / 10000000.0;
            var sampleCount = 256;
            var data1 = new float[sampleCount];
            var data2 = new float[sampleCount];
            var data3 = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                double phase = (i / (double)sampleCount) * Math.PI * 4 + time;
                data1[i] = (float)(Math.Sin(phase) * 0.5 + Math.Sin(phase * 2.5) * 0.3);
                data2[i] = (float)(Math.Sin(phase * 0.7 + 1) * 0.4 + Math.Sin(phase * 3) * 0.2);
                data3[i] = (float)(Math.Sin(phase * 0.4 + 2) * 0.3);
            }

            if (_showL1) DrawWaveform(g, data1, _l1Color, 0);
            if (_showL2) DrawWaveform(g, data2, _l2Color, 1);
            if (_showL3) DrawWaveform(g, data3, _l3Color, 2);
        }

        private void DrawLegend(Graphics g)
        {
            using var font = new Font("Consolas", 8);

            int x = 10;
            int y = 5;

            if (_showL1)
            {
                using var brush = new SolidBrush(_l1Color);
                g.FillRectangle(brush, x, y, 10, 10);
                g.DrawString("L1 Memory", font, brush, x + 15, y - 1);
                x += 85;
            }

            if (_showL2)
            {
                using var brush = new SolidBrush(_l2Color);
                g.FillRectangle(brush, x, y, 10, 10);
                g.DrawString("L2 Redis", font, brush, x + 15, y - 1);
                x += 75;
            }

            if (_showL3)
            {
                using var brush = new SolidBrush(_l3Color);
                g.FillRectangle(brush, x, y, 10, 10);
                g.DrawString("L3 Mongo", font, brush, x + 15, y - 1);
            }
        }

        private void DrawStats(Graphics g)
        {
            if (_monitor == null) return;

            using var font = new Font("Consolas", 8);
            using var brush = new SolidBrush(Color.FromArgb(150, 150, 150));

            var stats = $"Events/sec: {_monitor.EventsPerSecond}";
            var size = g.MeasureString(stats, font);
            g.DrawString(stats, font, brush, Width - size.Width - 10, Height - size.Height - 5);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
