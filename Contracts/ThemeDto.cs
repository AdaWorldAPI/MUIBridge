// File: MUIBridge/Contracts/ThemeDto.cs
// Purpose: Data Transfer Objects for theme configuration and exchange.
// Use these DTOs when loading themes from JSON/API or sharing between sessions.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;

namespace MUIBridge.Contracts
{
    /// <summary>
    /// DTO for theme definition - can be serialized to/from JSON.
    /// Use this to load themes from configuration files or APIs.
    /// </summary>
    public class ThemeDto
    {
        /// <summary>
        /// Unique theme identifier.
        /// Required. Must be unique across all registered themes.
        /// Examples: "Light", "Dark", "CorporateBlue", "AmigaWorkbench"
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable display name.
        /// Optional. Falls back to Name if not provided.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Theme description.
        /// Optional. Used in theme pickers/documentation.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Theme version for compatibility tracking.
        /// Format: "major.minor.patch" (e.g., "1.0.0")
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Theme author/source.
        /// </summary>
        [JsonPropertyName("author")]
        public string? Author { get; set; }

        // === Colors (Hex format: "#RRGGBB" or "#AARRGGBB") ===

        /// <summary>
        /// Primary brand color. Used for main interactive elements (buttons, links).
        /// Format: "#RRGGBB" or "#AARRGGBB"
        /// Example: "#007ACC" (blue)
        /// </summary>
        [JsonPropertyName("primaryColor")]
        public string PrimaryColor { get; set; } = "#007ACC";

        /// <summary>
        /// Secondary color. Used for supporting/less prominent elements.
        /// Format: "#RRGGBB"
        /// Example: "#CCCCCC" (light gray)
        /// </summary>
        [JsonPropertyName("secondaryColor")]
        public string SecondaryColor { get; set; } = "#CCCCCC";

        /// <summary>
        /// Background color for forms and controls.
        /// Format: "#RRGGBB"
        /// Example: "#FFFFFF" (white) for light theme, "#1C1C1E" for dark
        /// </summary>
        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// Default text color.
        /// Format: "#RRGGBB"
        /// Example: "#000000" (black) for light theme, "#FFFFFF" for dark
        /// </summary>
        [JsonPropertyName("textColor")]
        public string TextColor { get; set; } = "#000000";

        /// <summary>
        /// Border color for controls.
        /// Format: "#RRGGBB"
        /// Example: "#C8C8C8" (gray)
        /// </summary>
        [JsonPropertyName("borderColor")]
        public string BorderColor { get; set; } = "#C8C8C8";

        /// <summary>
        /// Accent color for highlights, mood effects, focus states.
        /// Format: "#RRGGBB"
        /// Example: "#0099FF" (bright blue)
        /// </summary>
        [JsonPropertyName("accentColor")]
        public string AccentColor { get; set; } = "#0099FF";

        /// <summary>
        /// Success state color (optional).
        /// Format: "#RRGGBB"
        /// Example: "#28A745" (green)
        /// </summary>
        [JsonPropertyName("successColor")]
        public string? SuccessColor { get; set; }

        /// <summary>
        /// Warning state color (optional).
        /// Format: "#RRGGBB"
        /// Example: "#FFC107" (amber)
        /// </summary>
        [JsonPropertyName("warningColor")]
        public string? WarningColor { get; set; }

        /// <summary>
        /// Error/danger state color (optional).
        /// Format: "#RRGGBB"
        /// Example: "#DC3545" (red)
        /// </summary>
        [JsonPropertyName("errorColor")]
        public string? ErrorColor { get; set; }

        // === Typography ===

        /// <summary>
        /// Default font family name.
        /// Example: "Segoe UI", "Arial", "Consolas"
        /// </summary>
        [JsonPropertyName("fontFamily")]
        public string FontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// Default font size in points.
        /// Example: 9.0, 10.0, 12.0
        /// </summary>
        [JsonPropertyName("fontSize")]
        public float FontSize { get; set; } = 9f;

        /// <summary>
        /// Header font size in points (optional).
        /// </summary>
        [JsonPropertyName("headerFontSize")]
        public float? HeaderFontSize { get; set; }

        // === Layout & Styling ===

        /// <summary>
        /// Default corner radius in pixels (0 for sharp corners).
        /// Example: 4.0 for subtle rounding, 0 for sharp/retro look
        /// </summary>
        [JsonPropertyName("cornerRadius")]
        public float CornerRadius { get; set; } = 4f;

        /// <summary>
        /// Default border thickness in pixels.
        /// Example: 1.0 for thin borders, 2.0 for prominent borders
        /// </summary>
        [JsonPropertyName("borderThickness")]
        public float BorderThickness { get; set; } = 1f;

        /// <summary>
        /// Default padding in pixels (optional).
        /// </summary>
        [JsonPropertyName("defaultPadding")]
        public int? DefaultPadding { get; set; }

        /// <summary>
        /// Default margin in pixels (optional).
        /// </summary>
        [JsonPropertyName("defaultMargin")]
        public int? DefaultMargin { get; set; }

        // === Mood Configuration ===

        /// <summary>
        /// Mood pulse color - target color when mood is high.
        /// If not specified, derived from AccentColor.
        /// Format: "#RRGGBB"
        /// </summary>
        [JsonPropertyName("moodPulseColor")]
        public string? MoodPulseColor { get; set; }

        /// <summary>
        /// Maximum blend factor for mood effects (0.0 to 1.0).
        /// Higher = more dramatic mood color shifts.
        /// Default: 0.6
        /// </summary>
        [JsonPropertyName("moodBlendFactor")]
        public float MoodBlendFactor { get; set; } = 0.6f;

        // === Component-Specific Overrides ===

        /// <summary>
        /// Optional component-specific style overrides.
        /// Key format: "ComponentName.Property" (e.g., "Button.HoverColor")
        /// </summary>
        [JsonPropertyName("componentStyles")]
        public Dictionary<string, string>? ComponentStyles { get; set; }

        // === Metadata ===

        /// <summary>
        /// Theme category for grouping in UI.
        /// Examples: "Light", "Dark", "High Contrast", "Retro", "Corporate"
        /// </summary>
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        /// <summary>
        /// Tags for search/filter.
        /// Examples: ["modern", "minimal"], ["retro", "amiga", "80s"]
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Whether this theme is dark (for system preference matching).
        /// </summary>
        [JsonPropertyName("isDark")]
        public bool IsDark { get; set; } = false;
    }

    /// <summary>
    /// DTO for theme color palette - minimal version for quick theming.
    /// </summary>
    public class ThemeColorsDto
    {
        [JsonPropertyName("primary")]
        public string Primary { get; set; } = "#007ACC";

        [JsonPropertyName("secondary")]
        public string Secondary { get; set; } = "#CCCCCC";

        [JsonPropertyName("background")]
        public string Background { get; set; } = "#FFFFFF";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "#000000";

        [JsonPropertyName("border")]
        public string Border { get; set; } = "#C8C8C8";

        [JsonPropertyName("accent")]
        public string Accent { get; set; } = "#0099FF";
    }

    /// <summary>
    /// DTO for runtime theme state (includes mood).
    /// </summary>
    public class ThemeStateDto
    {
        /// <summary>
        /// Currently active theme name.
        /// </summary>
        [JsonPropertyName("activeThemeName")]
        public string ActiveThemeName { get; set; } = string.Empty;

        /// <summary>
        /// Current mood intensity (0.0 to 1.0).
        /// </summary>
        [JsonPropertyName("moodIntensity")]
        public float MoodIntensity { get; set; }

        /// <summary>
        /// Current mood label.
        /// </summary>
        [JsonPropertyName("moodLabel")]
        public string MoodLabel { get; set; } = "Neutral";

        /// <summary>
        /// List of available theme names.
        /// </summary>
        [JsonPropertyName("availableThemes")]
        public List<string> AvailableThemes { get; set; } = new();

        /// <summary>
        /// Timestamp of last mood update.
        /// </summary>
        [JsonPropertyName("lastMoodUpdate")]
        public DateTime LastMoodUpdate { get; set; }
    }
}
