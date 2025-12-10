// File: MUIBridge/Themes/AmigaWorkbenchTheme.cs
// Purpose: Nostalgic Amiga Workbench 1.x inspired theme.
// Pays homage to the legendary Magic User Interface (MUI) aesthetic.

using System.Drawing;
using MUIBridge.Core;
using MUIBridge.Utils;

namespace MUIBridge.Themes
{
    /// <summary>
    /// Amiga Workbench 1.x inspired theme.
    /// Classic orange/blue/white/black 4-color palette.
    /// For those who remember the magic of the Amiga.
    /// </summary>
    public class AmigaWorkbenchTheme : ITheme
    {
        public string Name => "AmigaWorkbench";

        // Classic Workbench 1.x palette
        public Color PrimaryColor => Color.FromArgb(255, 136, 0);      // #FF8800 - Workbench Orange
        public Color SecondaryColor => Color.FromArgb(0, 85, 170);     // #0055AA - Workbench Blue
        public Color BackgroundColor => Color.FromArgb(0, 85, 170);    // #0055AA - Blue background
        public Color TextColor => Color.FromArgb(255, 255, 255);       // #FFFFFF - White
        public Color BorderColor => Color.FromArgb(0, 0, 0);           // #000000 - Black borders
        public Color AccentColor => Color.FromArgb(255, 136, 0);       // #FF8800 - Orange accent

        // Typography - Topaz was the Amiga system font
        public string DefaultFontName => "Consolas"; // Closest modern equivalent to Topaz
        public float DefaultFontSize => 10f;

        // Layout - Sharp corners for that authentic 80s look
        public float CornerRadius => 0f;
        public float BorderThickness => 2f;

        // Mood
        public float PredictionMoodIntensity { get; set; } = 0f;

        public Color GetCurrentPulseColor()
        {
            // Pulse between orange and white for that classic blinky effect
            Color pulseTarget = Color.FromArgb(255, 255, 255);
            return WaveFXColorBlender.Lerp(PrimaryColor, pulseTarget, PredictionMoodIntensity);
        }

        public object? GetComponentStyle(string componentKey)
        {
            // Could return specific Amiga-style patterns or icons
            return null;
        }
    }
}
