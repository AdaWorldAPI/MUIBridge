// File: MUIBridge/Contracts/ConfigurationDto.cs
// Purpose: Configuration DTOs for MUIBridge initialization and runtime settings.
// Use these when setting up MUIBridge via config files or remote configuration.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MUIBridge.Contracts
{
    /// <summary>
    /// Root configuration DTO for MUIBridge.
    /// Can be loaded from appsettings.json or remote API.
    /// </summary>
    public class MUIBridgeConfigDto
    {
        /// <summary>
        /// Configuration version for compatibility.
        /// </summary>
        [JsonPropertyName("configVersion")]
        public string ConfigVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Name of the default theme to apply on startup.
        /// Must match a theme name in Themes collection.
        /// </summary>
        [JsonPropertyName("defaultTheme")]
        public string DefaultTheme { get; set; } = "Light";

        /// <summary>
        /// Whether to follow system dark/light mode preference.
        /// </summary>
        [JsonPropertyName("followSystemTheme")]
        public bool FollowSystemTheme { get; set; } = false;

        /// <summary>
        /// Theme definitions.
        /// </summary>
        [JsonPropertyName("themes")]
        public List<ThemeDto>? Themes { get; set; }

        /// <summary>
        /// Prediction layer configuration.
        /// </summary>
        [JsonPropertyName("prediction")]
        public PredictionConfigDto Prediction { get; set; } = new();

        /// <summary>
        /// Logging configuration.
        /// </summary>
        [JsonPropertyName("logging")]
        public LoggingConfigDto Logging { get; set; } = new();

        /// <summary>
        /// Performance settings.
        /// </summary>
        [JsonPropertyName("performance")]
        public PerformanceConfigDto Performance { get; set; } = new();

        /// <summary>
        /// Feature flags.
        /// </summary>
        [JsonPropertyName("features")]
        public FeatureFlagsDto Features { get; set; } = new();
    }

    /// <summary>
    /// Prediction layer configuration.
    /// </summary>
    public class PredictionConfigDto
    {
        /// <summary>
        /// Whether prediction/mood features are enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Auto-start the mood adapter on application launch.
        /// </summary>
        [JsonPropertyName("autoStart")]
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// Minimum mood change threshold (0.0-1.0) before UI update.
        /// Higher = less frequent updates, lower = more responsive.
        /// </summary>
        [JsonPropertyName("moodChangeThreshold")]
        public float MoodChangeThreshold { get; set; } = 0.01f;

        /// <summary>
        /// Mood smoothing factor (0.0-1.0).
        /// Higher = smoother transitions, lower = more immediate.
        /// </summary>
        [JsonPropertyName("moodSmoothingFactor")]
        public float MoodSmoothingFactor { get; set; } = 0.2f;

        /// <summary>
        /// Mood decay rate per second when no predictions arrive.
        /// 0 = no decay, 0.1 = 10% decay per second.
        /// </summary>
        [JsonPropertyName("moodDecayRate")]
        public float MoodDecayRate { get; set; } = 0.05f;

        /// <summary>
        /// Maximum mood value (can be < 1.0 to limit intensity).
        /// </summary>
        [JsonPropertyName("maxMoodIntensity")]
        public float MaxMoodIntensity { get; set; } = 1.0f;

        /// <summary>
        /// External prediction service endpoint (optional).
        /// If set, predictions are fetched from this API.
        /// </summary>
        [JsonPropertyName("predictionServiceUrl")]
        public string? PredictionServiceUrl { get; set; }

        /// <summary>
        /// API key for prediction service (optional).
        /// </summary>
        [JsonPropertyName("predictionApiKey")]
        public string? PredictionApiKey { get; set; }

        /// <summary>
        /// Telemetry endpoint for sending user signals (optional).
        /// </summary>
        [JsonPropertyName("telemetryEndpoint")]
        public string? TelemetryEndpoint { get; set; }

        /// <summary>
        /// Whether to send telemetry signals.
        /// </summary>
        [JsonPropertyName("sendTelemetry")]
        public bool SendTelemetry { get; set; } = false;

        /// <summary>
        /// Batch size for telemetry signals.
        /// </summary>
        [JsonPropertyName("telemetryBatchSize")]
        public int TelemetryBatchSize { get; set; } = 10;

        /// <summary>
        /// Telemetry flush interval in milliseconds.
        /// </summary>
        [JsonPropertyName("telemetryFlushIntervalMs")]
        public int TelemetryFlushIntervalMs { get; set; } = 5000;
    }

    /// <summary>
    /// Logging configuration.
    /// </summary>
    public class LoggingConfigDto
    {
        /// <summary>
        /// Minimum log level: "Trace", "Debug", "Info", "Warning", "Error"
        /// </summary>
        [JsonPropertyName("minLevel")]
        public string MinLevel { get; set; } = "Information";

        /// <summary>
        /// Whether to log theme changes.
        /// </summary>
        [JsonPropertyName("logThemeChanges")]
        public bool LogThemeChanges { get; set; } = true;

        /// <summary>
        /// Whether to log mood changes.
        /// </summary>
        [JsonPropertyName("logMoodChanges")]
        public bool LogMoodChanges { get; set; } = false;

        /// <summary>
        /// Whether to log prediction packets.
        /// </summary>
        [JsonPropertyName("logPredictions")]
        public bool LogPredictions { get; set; } = false;

        /// <summary>
        /// Whether to log user signals (telemetry).
        /// Caution: May include sensitive interaction data.
        /// </summary>
        [JsonPropertyName("logSignals")]
        public bool LogSignals { get; set; } = false;
    }

    /// <summary>
    /// Performance tuning configuration.
    /// </summary>
    public class PerformanceConfigDto
    {
        /// <summary>
        /// Enable double buffering for all controls.
        /// </summary>
        [JsonPropertyName("doubleBuffering")]
        public bool DoubleBuffering { get; set; } = true;

        /// <summary>
        /// Maximum UI updates per second (throttling).
        /// 0 = unlimited, 60 = 60fps cap
        /// </summary>
        [JsonPropertyName("maxUiUpdatesPerSecond")]
        public int MaxUiUpdatesPerSecond { get; set; } = 60;

        /// <summary>
        /// Debounce mood updates (milliseconds).
        /// </summary>
        [JsonPropertyName("moodUpdateDebounceMs")]
        public int MoodUpdateDebounceMs { get; set; } = 16; // ~60fps

        /// <summary>
        /// Use hardware acceleration where available.
        /// </summary>
        [JsonPropertyName("useHardwareAcceleration")]
        public bool UseHardwareAcceleration { get; set; } = true;

        /// <summary>
        /// Cache rendered assets (brushes, pens).
        /// </summary>
        [JsonPropertyName("cacheRenderAssets")]
        public bool CacheRenderAssets { get; set; } = true;
    }

    /// <summary>
    /// Feature flags for enabling/disabling functionality.
    /// </summary>
    public class FeatureFlagsDto
    {
        /// <summary>
        /// Enable mood reactivity on controls.
        /// </summary>
        [JsonPropertyName("moodReactivity")]
        public bool MoodReactivity { get; set; } = true;

        /// <summary>
        /// Enable theme switching.
        /// </summary>
        [JsonPropertyName("themeSwitching")]
        public bool ThemeSwitching { get; set; } = true;

        /// <summary>
        /// Enable focus animations.
        /// </summary>
        [JsonPropertyName("focusAnimations")]
        public bool FocusAnimations { get; set; } = true;

        /// <summary>
        /// Enable hover effects.
        /// </summary>
        [JsonPropertyName("hoverEffects")]
        public bool HoverEffects { get; set; } = true;

        /// <summary>
        /// Enable click ripple effects.
        /// </summary>
        [JsonPropertyName("rippleEffects")]
        public bool RippleEffects { get; set; } = false;

        /// <summary>
        /// Enable telemetry collection.
        /// </summary>
        [JsonPropertyName("telemetryCollection")]
        public bool TelemetryCollection { get; set; } = false;

        /// <summary>
        /// Enable OptiScope debug overlay.
        /// </summary>
        [JsonPropertyName("optiScopeOverlay")]
        public bool OptiScopeOverlay { get; set; } = false;

        /// <summary>
        /// Additional custom feature flags.
        /// </summary>
        [JsonPropertyName("custom")]
        public Dictionary<string, bool>? Custom { get; set; }
    }

    /// <summary>
    /// DTO for control-specific configuration.
    /// </summary>
    public class ControlConfigDto
    {
        /// <summary>
        /// Control type this applies to.
        /// Examples: "Button", "TextBox", "DataGridView", "*" for all
        /// </summary>
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = "*";

        /// <summary>
        /// Whether mood reactivity is enabled for this control type.
        /// </summary>
        [JsonPropertyName("moodReactive")]
        public bool MoodReactive { get; set; } = true;

        /// <summary>
        /// Custom mood blend factor for this control (overrides theme).
        /// </summary>
        [JsonPropertyName("moodBlendFactor")]
        public float? MoodBlendFactor { get; set; }

        /// <summary>
        /// Whether to show focus animations.
        /// </summary>
        [JsonPropertyName("showFocusAnimation")]
        public bool ShowFocusAnimation { get; set; } = true;

        /// <summary>
        /// Custom corner radius (overrides theme).
        /// </summary>
        [JsonPropertyName("cornerRadius")]
        public float? CornerRadius { get; set; }

        /// <summary>
        /// Custom border thickness (overrides theme).
        /// </summary>
        [JsonPropertyName("borderThickness")]
        public float? BorderThickness { get; set; }
    }
}
