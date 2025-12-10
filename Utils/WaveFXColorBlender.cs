// File: MUIBridge/Utils/WaveFXColorBlender.cs
// Purpose: Color blending utilities for smooth mood-reactive visual transitions.
// Named after the "Wave" in WaveMoodAdapter - creates fluid color animations.

using System;
using System.Drawing;

namespace MUIBridge.Utils
{
    /// <summary>
    /// Provides color blending utilities for mood-reactive UI effects.
    /// Optimized for WinForms GDI+ rendering performance.
    /// </summary>
    public static class WaveFXColorBlender
    {
        /// <summary>
        /// Linear interpolation between two colors.
        /// </summary>
        /// <param name="start">Starting color (at amount=0).</param>
        /// <param name="end">Ending color (at amount=1).</param>
        /// <param name="amount">Blend amount from 0.0 to 1.0.</param>
        /// <returns>Blended color.</returns>
        public static Color Lerp(Color start, Color end, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            int a = (int)(start.A + (end.A - start.A) * amount);
            int r = (int)(start.R + (end.R - start.R) * amount);
            int g = (int)(start.G + (end.G - start.G) * amount);
            int b = (int)(start.B + (end.B - start.B) * amount);

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Smoothstep interpolation for more natural transitions.
        /// </summary>
        public static Color SmoothLerp(Color start, Color end, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            // Smoothstep formula: 3x² - 2x³
            float smoothed = amount * amount * (3f - 2f * amount);
            return Lerp(start, end, smoothed);
        }

        /// <summary>
        /// Creates a pulsing effect by oscillating between two colors.
        /// </summary>
        /// <param name="baseColor">Base color.</param>
        /// <param name="pulseColor">Color to pulse toward.</param>
        /// <param name="intensity">Pulse intensity (0-1).</param>
        /// <param name="phase">Current phase in radians (use DateTime for animation).</param>
        /// <returns>Pulsing color.</returns>
        public static Color Pulse(Color baseColor, Color pulseColor, float intensity, double phase)
        {
            float pulse = (float)((Math.Sin(phase) + 1.0) / 2.0); // 0 to 1
            return Lerp(baseColor, pulseColor, pulse * intensity);
        }

        /// <summary>
        /// Adjusts color brightness.
        /// </summary>
        /// <param name="color">Source color.</param>
        /// <param name="factor">Factor > 1 brightens, < 1 darkens.</param>
        public static Color AdjustBrightness(Color color, float factor)
        {
            int r = Math.Clamp((int)(color.R * factor), 0, 255);
            int g = Math.Clamp((int)(color.G * factor), 0, 255);
            int b = Math.Clamp((int)(color.B * factor), 0, 255);
            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Converts hex string to Color (for theme configuration files).
        /// </summary>
        /// <param name="hex">Hex color string (e.g., "#FF5500" or "FF5500").</param>
        /// <returns>Parsed Color or Color.Empty if invalid.</returns>
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Color.Empty;

            hex = hex.TrimStart('#');

            try
            {
                if (hex.Length == 6)
                {
                    return Color.FromArgb(
                        255,
                        Convert.ToInt32(hex.Substring(0, 2), 16),
                        Convert.ToInt32(hex.Substring(2, 2), 16),
                        Convert.ToInt32(hex.Substring(4, 2), 16));
                }
                else if (hex.Length == 8)
                {
                    return Color.FromArgb(
                        Convert.ToInt32(hex.Substring(0, 2), 16),
                        Convert.ToInt32(hex.Substring(2, 2), 16),
                        Convert.ToInt32(hex.Substring(4, 2), 16),
                        Convert.ToInt32(hex.Substring(6, 2), 16));
                }
            }
            catch
            {
                // Invalid hex format
            }

            return Color.Empty;
        }

        /// <summary>
        /// Converts Color to hex string.
        /// </summary>
        public static string ToHex(Color color, bool includeAlpha = false)
        {
            if (includeAlpha)
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
