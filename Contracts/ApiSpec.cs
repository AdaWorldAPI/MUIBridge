// File: MUIBridge/Contracts/ApiSpec.cs
// Purpose: API specification and service contracts for MUIBridge integration.
// These define the public API surface for other sessions/applications.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MUIBridge.Contracts
{
    // ============================================================================
    // CORE SERVICE INTERFACES
    // ============================================================================

    /// <summary>
    /// Main entry point for MUIBridge theming operations.
    /// Inject this interface to control themes from external code.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the currently active theme name.
        /// </summary>
        string CurrentThemeName { get; }

        /// <summary>
        /// Gets the current theme state including mood.
        /// </summary>
        ThemeStateDto GetCurrentState();

        /// <summary>
        /// Gets all available theme names.
        /// </summary>
        IReadOnlyList<string> GetAvailableThemes();

        /// <summary>
        /// Gets a theme definition by name.
        /// </summary>
        ThemeDto? GetTheme(string themeName);

        /// <summary>
        /// Sets the active theme.
        /// </summary>
        /// <param name="themeName">Name of theme to activate.</param>
        /// <returns>True if successful.</returns>
        bool SetTheme(string themeName);

        /// <summary>
        /// Registers a new theme at runtime.
        /// </summary>
        /// <param name="theme">Theme definition.</param>
        /// <returns>True if registered successfully.</returns>
        bool RegisterTheme(ThemeDto theme);

        /// <summary>
        /// Event raised when theme changes.
        /// </summary>
        event EventHandler<ThemeChangedEventDto>? ThemeChanged;
    }

    /// <summary>
    /// Service for mood/prediction operations.
    /// Inject this to control mood reactivity.
    /// </summary>
    public interface IMoodService
    {
        /// <summary>
        /// Gets the current mood intensity (0.0-1.0).
        /// </summary>
        float CurrentMoodIntensity { get; }

        /// <summary>
        /// Gets the current mood state.
        /// </summary>
        MoodStateDto GetCurrentMood();

        /// <summary>
        /// Gets mood for a specific context.
        /// </summary>
        MoodStateDto GetMoodForContext(string contextId);

        /// <summary>
        /// Manually sets mood intensity.
        /// </summary>
        /// <param name="intensity">Intensity value (0.0-1.0).</param>
        void SetMoodIntensity(float intensity);

        /// <summary>
        /// Resets mood to neutral.
        /// </summary>
        void ResetMood();

        /// <summary>
        /// Applies a prediction to update mood.
        /// </summary>
        Task ApplyPredictionAsync(PredictionDto prediction);

        /// <summary>
        /// Event raised when mood changes.
        /// </summary>
        event EventHandler<MoodChangedEventDto>? MoodChanged;
    }

    /// <summary>
    /// Service for prediction telemetry operations.
    /// Inject this to send/receive predictions.
    /// </summary>
    public interface IPredictionService
    {
        /// <summary>
        /// Publishes a user interaction signal.
        /// </summary>
        Task PublishSignalAsync(UserSignalDto signal, CancellationToken ct = default);

        /// <summary>
        /// Publishes a prediction packet.
        /// </summary>
        Task PublishPredictionAsync(PredictionDto prediction, CancellationToken ct = default);

        /// <summary>
        /// Requests a prediction for a given signal.
        /// </summary>
        Task<PredictionDto?> GetPredictionAsync(UserSignalDto signal, CancellationToken ct = default);

        /// <summary>
        /// Event raised when prediction is received.
        /// </summary>
        event Func<PredictionDto, Task>? PredictionReceived;

        /// <summary>
        /// Event raised when signal is captured.
        /// </summary>
        event Func<UserSignalDto, Task>? SignalCaptured;
    }

    /// <summary>
    /// Service for configuration management.
    /// Inject this to modify runtime configuration.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        MUIBridgeConfigDto GetConfiguration();

        /// <summary>
        /// Updates configuration at runtime.
        /// </summary>
        void UpdateConfiguration(MUIBridgeConfigDto config);

        /// <summary>
        /// Gets a specific feature flag value.
        /// </summary>
        bool GetFeatureFlag(string flagName);

        /// <summary>
        /// Sets a feature flag at runtime.
        /// </summary>
        void SetFeatureFlag(string flagName, bool enabled);

        /// <summary>
        /// Reloads configuration from source.
        /// </summary>
        Task ReloadAsync(CancellationToken ct = default);
    }

    // ============================================================================
    // INTEGRATION HELPER CONTRACTS
    // ============================================================================

    /// <summary>
    /// Helper for quick MUIBridge integration.
    /// Use this for simplified access to all services.
    /// </summary>
    public interface IMUIBridgeFacade
    {
        /// <summary>
        /// Theme service instance.
        /// </summary>
        IThemeService Themes { get; }

        /// <summary>
        /// Mood service instance.
        /// </summary>
        IMoodService Mood { get; }

        /// <summary>
        /// Prediction service instance.
        /// </summary>
        IPredictionService Predictions { get; }

        /// <summary>
        /// Configuration service instance.
        /// </summary>
        IConfigurationService Configuration { get; }

        /// <summary>
        /// Quick access: Set theme by name.
        /// </summary>
        bool SetTheme(string themeName);

        /// <summary>
        /// Quick access: Set mood intensity.
        /// </summary>
        void SetMood(float intensity);

        /// <summary>
        /// Quick access: Publish prediction.
        /// </summary>
        Task PublishPredictionAsync(float confidence, string predictedValue);

        /// <summary>
        /// Quick access: Get current state summary.
        /// </summary>
        MUIBridgeStateDto GetState();
    }

    /// <summary>
    /// Combined state DTO for quick status checks.
    /// </summary>
    public class MUIBridgeStateDto
    {
        public string ActiveTheme { get; set; } = string.Empty;
        public float MoodIntensity { get; set; }
        public string MoodLabel { get; set; } = "Neutral";
        public int AvailableThemeCount { get; set; }
        public bool PredictionEnabled { get; set; }
        public DateTime LastPredictionTime { get; set; }
        public DateTime LastMoodUpdate { get; set; }
        public bool IsHealthy { get; set; } = true;
    }

    // ============================================================================
    // FACTORY CONTRACTS
    // ============================================================================

    /// <summary>
    /// Factory for creating MUIBridge controls.
    /// Use this for creating controls with proper DI.
    /// </summary>
    public interface IMUIControlFactory
    {
        /// <summary>
        /// Creates a themed button.
        /// </summary>
        object CreateButton();

        /// <summary>
        /// Creates a themed label.
        /// </summary>
        object CreateLabel();

        /// <summary>
        /// Creates a themed textbox.
        /// </summary>
        object CreateTextBox();

        /// <summary>
        /// Creates a themed checkbox.
        /// </summary>
        object CreateCheckBox();

        /// <summary>
        /// Creates a themed combobox.
        /// </summary>
        object CreateComboBox();

        /// <summary>
        /// Creates a themed panel.
        /// </summary>
        object CreatePanel();

        /// <summary>
        /// Creates a themed data grid.
        /// </summary>
        object CreateDataGridView();

        /// <summary>
        /// Creates a themed progress bar.
        /// </summary>
        object CreateProgressBar();

        /// <summary>
        /// Creates a control by type name.
        /// </summary>
        /// <param name="controlTypeName">Type name (e.g., "Button", "Label").</param>
        object? CreateControl(string controlTypeName);
    }

    // ============================================================================
    // REST API CONTRACTS (for external services)
    // ============================================================================

    /// <summary>
    /// REST API request for setting theme.
    /// POST /api/muibridge/theme
    /// </summary>
    public class SetThemeRequest
    {
        public string ThemeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// REST API request for setting mood.
    /// POST /api/muibridge/mood
    /// </summary>
    public class SetMoodRequest
    {
        public float Intensity { get; set; }
        public string? Label { get; set; }
        public string? ContextId { get; set; }
    }

    /// <summary>
    /// REST API request for publishing prediction.
    /// POST /api/muibridge/prediction
    /// </summary>
    public class PublishPredictionRequest
    {
        public float Confidence { get; set; }
        public string PredictedValue { get; set; } = string.Empty;
        public string? SourceModel { get; set; }
        public string? TargetContext { get; set; }
        public float? MoodIntensity { get; set; }
        public string? MoodLabel { get; set; }
    }

    /// <summary>
    /// REST API response wrapper.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Standard API error codes.
    /// </summary>
    public static class ApiErrorCodes
    {
        public const string ThemeNotFound = "THEME_NOT_FOUND";
        public const string InvalidMoodValue = "INVALID_MOOD_VALUE";
        public const string PredictionServiceUnavailable = "PREDICTION_UNAVAILABLE";
        public const string ConfigurationError = "CONFIG_ERROR";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string InternalError = "INTERNAL_ERROR";
    }
}
