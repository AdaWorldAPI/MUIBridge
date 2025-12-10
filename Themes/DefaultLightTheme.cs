// File: MUIBridge/Themes/DefaultLightTheme.cs
// Purpose: Clean, modern light theme - good for business applications.

using System.Drawing;
using MUIBridge.Core;
using MUIBridge.Utils;

namespace MUIBridge.Themes
{
    /// <summary>
    /// Default light theme with Material Design-inspired colors.
    /// Suitable for standard business applications.
    /// </summary>
    public class DefaultLightTheme : ITheme
    {
        public string Name => "Light";

        // Colors
        public Color PrimaryColor => Color.FromArgb(0, 122, 204);      // #007ACC - Blue
        public Color SecondaryColor => Color.FromArgb(204, 204, 204);  // #CCCCCC - Light Gray
        public Color BackgroundColor => Color.FromArgb(255, 255, 255); // #FFFFFF - White
        public Color TextColor => Color.FromArgb(0, 0, 0);             // #000000 - Black
        public Color BorderColor => Color.FromArgb(200, 200, 200);     // #C8C8C8 - Border Gray
        public Color AccentColor => Color.FromArgb(0, 153, 255);       // #0099FF - Bright Blue

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
            // Blend from primary toward accent based on mood intensity
            return WaveFXColorBlender.Lerp(PrimaryColor, AccentColor, PredictionMoodIntensity * 0.7f);
        }

        public object? GetComponentStyle(string componentKey)
        {
            // Extensibility point for component-specific styles
            return null;
        }
    }
}
