// File: MUIBridge/Core/ITheme.cs
// Purpose: Core theme contract for WinForms-compatible drop-in replacement.
// Design: Uses System.Drawing.Color for 100% WinForms syntax compatibility.

using System.Drawing;

namespace MUIBridge.Core
{
    /// <summary>
    /// Defines the contract for MUIBridge themes.
    /// Uses System.Drawing.Color to ensure drop-in compatibility with legacy WinForms code.
    /// </summary>
    public interface ITheme
    {
        /// <summary>
        /// Unique theme identifier (e.g., "Light", "Dark", "Amiga", "CyberPunk").
        /// </summary>
        string Name { get; }

        // --- Core Colors (WinForms Compatible) ---

        /// <summary>Primary brand color for main interactive elements.</summary>
        Color PrimaryColor { get; }

        /// <summary>Secondary color for supporting elements.</summary>
        Color SecondaryColor { get; }

        /// <summary>Form/control background color.</summary>
        Color BackgroundColor { get; }

        /// <summary>Default text color.</summary>
        Color TextColor { get; }

        /// <summary>Border color for controls.</summary>
        Color BorderColor { get; }

        /// <summary>Accent color for highlights, mood effects, and emphasis.</summary>
        Color AccentColor { get; }

        // --- Typography ---

        /// <summary>Default font family name (e.g., "Segoe UI", "Topaz" for Amiga style).</summary>
        string DefaultFontName { get; }

        /// <summary>Default font size in points.</summary>
        float DefaultFontSize { get; }

        // --- Layout & Styling ---

        /// <summary>Default corner radius for rounded controls (0 for sharp corners).</summary>
        float CornerRadius { get; }

        /// <summary>Default border thickness in pixels.</summary>
        float BorderThickness { get; }

        // --- Mood/Prediction Reactivity ---

        /// <summary>
        /// Current prediction mood intensity (0.0 to 1.0).
        /// Updated by WaveMoodAdapter based on AI prediction confidence.
        /// </summary>
        float PredictionMoodIntensity { get; set; }

        /// <summary>
        /// Calculates the current "pulse" color based on mood intensity.
        /// Used by mood-reactive controls to blend their appearance.
        /// </summary>
        /// <returns>Color blended between primary and accent based on mood.</returns>
        Color GetCurrentPulseColor();

        /// <summary>
        /// Gets component-specific style overrides (optional extensibility).
        /// </summary>
        /// <param name="componentKey">Component identifier (e.g., "Button.Hover", "TextBox.Focus").</param>
        /// <returns>Style object or null if no override.</returns>
        object? GetComponentStyle(string componentKey);
    }
}
