// File: MUIBridge/Themes/DefaultDarkTheme.cs
// Purpose: Modern dark theme - easier on the eyes for extended use.

using System.Drawing;
using MUIBridge.Core;
using MUIBridge.Utils;

namespace MUIBridge.Themes
{
    /// <summary>
    /// Dark theme inspired by modern IDEs and applications.
    /// Reduces eye strain for extended use.
    /// </summary>
    public class DefaultDarkTheme : ITheme
    {
        public string Name => "Dark";

        // Colors
        public Color PrimaryColor => Color.FromArgb(10, 132, 255);     // #0A84FF - iOS Blue
        public Color SecondaryColor => Color.FromArgb(58, 58, 60);     // #3A3A3C - Dark Gray
        public Color BackgroundColor => Color.FromArgb(28, 28, 30);    // #1C1C1E - Near Black
        public Color TextColor => Color.FromArgb(255, 255, 255);       // #FFFFFF - White
        public Color BorderColor => Color.FromArgb(72, 72, 74);        // #48484A - Border Gray
        public Color AccentColor => Color.FromArgb(94, 230, 94);       // #5EE65E - Green

        // Typography
        public string DefaultFontName => "Segoe UI";
        public float DefaultFontSize => 9f;

        // Layout
        public float CornerRadius => 4f;
        public float BorderThickness => 1f;

        // Mood
        public float PredictionMoodIntensity { get; set; } = 0f;

        public Color GetCurrentPulseColor()
        {
            return WaveFXColorBlender.Lerp(PrimaryColor, AccentColor, PredictionMoodIntensity * 0.7f);
        }

        public object? GetComponentStyle(string componentKey) => null;
    }
}
