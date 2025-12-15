// File: MUIBridge/Contracts/EventContracts.cs
// Purpose: Event contracts and callback signatures for MUIBridge integration.
// Use these when subscribing to MUIBridge events from external systems.

using System;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MUIBridge.Contracts
{
    // ============================================================================
    // EVENT ARGUMENT DTOS
    // ============================================================================

    /// <summary>
    /// Event arguments for theme changes.
    /// </summary>
    public class ThemeChangedEventDto
    {
        /// <summary>
        /// Name of the previous theme.
        /// </summary>
        [JsonPropertyName("previousTheme")]
        public string PreviousTheme { get; set; } = string.Empty;

        /// <summary>
        /// Name of the new (current) theme.
        /// </summary>
        [JsonPropertyName("newTheme")]
        public string NewTheme { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the change.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether mood was reset during theme change.
        /// </summary>
        [JsonPropertyName("moodReset")]
        public bool MoodReset { get; set; } = true;

        /// <summary>
        /// Source of the change: "User", "System", "API", "Config"
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; } = "User";
    }

    /// <summary>
    /// Event arguments for mood changes.
    /// </summary>
    public class MoodChangedEventDto
    {
        /// <summary>
        /// Previous mood intensity.
        /// </summary>
        [JsonPropertyName("previousIntensity")]
        public float PreviousIntensity { get; set; }

        /// <summary>
        /// New mood intensity.
        /// </summary>
        [JsonPropertyName("newIntensity")]
        public float NewIntensity { get; set; }

        /// <summary>
        /// Mood label.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; } = "Neutral";

        /// <summary>
        /// Delta (change amount).
        /// </summary>
        [JsonPropertyName("delta")]
        public float Delta { get; set; }

        /// <summary>
        /// Timestamp of the change.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID of the prediction that caused this change.
        /// </summary>
        [JsonPropertyName("sourcePredictionId")]
        public string? SourcePredictionId { get; set; }

        /// <summary>
        /// Context ID where mood changed.
        /// </summary>
        [JsonPropertyName("contextId")]
        public string ContextId { get; set; } = "global";
    }

    /// <summary>
    /// Event arguments for prediction received.
    /// </summary>
    public class PredictionReceivedEventDto
    {
        /// <summary>
        /// The prediction packet.
        /// </summary>
        [JsonPropertyName("prediction")]
        public PredictionDto Prediction { get; set; } = new();

        /// <summary>
        /// Whether this prediction was applied to mood.
        /// </summary>
        [JsonPropertyName("appliedToMood")]
        public bool AppliedToMood { get; set; }

        /// <summary>
        /// Reason if not applied (e.g., "BelowThreshold", "Expired")
        /// </summary>
        [JsonPropertyName("skipReason")]
        public string? SkipReason { get; set; }

        /// <summary>
        /// Processing latency in milliseconds.
        /// </summary>
        [JsonPropertyName("processingLatencyMs")]
        public long ProcessingLatencyMs { get; set; }
    }

    /// <summary>
    /// Event arguments for user signal captured.
    /// </summary>
    public class SignalCapturedEventDto
    {
        /// <summary>
        /// The captured signal.
        /// </summary>
        [JsonPropertyName("signal")]
        public UserSignalDto Signal { get; set; } = new();

        /// <summary>
        /// Whether signal was sent to prediction service.
        /// </summary>
        [JsonPropertyName("sentToPrediction")]
        public bool SentToPrediction { get; set; }

        /// <summary>
        /// Whether signal was sent to telemetry.
        /// </summary>
        [JsonPropertyName("sentToTelemetry")]
        public bool SentToTelemetry { get; set; }
    }

    /// <summary>
    /// Event arguments for control state changes.
    /// </summary>
    public class ControlStateChangedEventDto
    {
        /// <summary>
        /// Control identifier.
        /// </summary>
        [JsonPropertyName("controlId")]
        public string ControlId { get; set; } = string.Empty;

        /// <summary>
        /// Control type.
        /// </summary>
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;

        /// <summary>
        /// State that changed: "Theme", "Mood", "Focus", "Enabled", "Visible"
        /// </summary>
        [JsonPropertyName("stateChanged")]
        public string StateChanged { get; set; } = string.Empty;

        /// <summary>
        /// Previous value (if applicable).
        /// </summary>
        [JsonPropertyName("previousValue")]
        public object? PreviousValue { get; set; }

        /// <summary>
        /// New value.
        /// </summary>
        [JsonPropertyName("newValue")]
        public object? NewValue { get; set; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // ============================================================================
    // CALLBACK DELEGATE SIGNATURES
    // ============================================================================

    /// <summary>
    /// Callback signature for theme change events.
    /// </summary>
    public delegate void ThemeChangedCallback(ThemeChangedEventDto args);

    /// <summary>
    /// Callback signature for mood change events.
    /// </summary>
    public delegate void MoodChangedCallback(MoodChangedEventDto args);

    /// <summary>
    /// Async callback signature for prediction events.
    /// </summary>
    public delegate Task PredictionReceivedCallbackAsync(PredictionReceivedEventDto args);

    /// <summary>
    /// Async callback signature for signal events.
    /// </summary>
    public delegate Task SignalCapturedCallbackAsync(SignalCapturedEventDto args);

    // ============================================================================
    // EVENT SUBSCRIPTION CONTRACTS
    // ============================================================================

    /// <summary>
    /// Contract for subscribing to MUIBridge events.
    /// Implement this to receive all MUIBridge events.
    /// </summary>
    public interface IMUIBridgeEventSubscriber
    {
        /// <summary>
        /// Called when the active theme changes.
        /// </summary>
        void OnThemeChanged(ThemeChangedEventDto args);

        /// <summary>
        /// Called when mood intensity changes.
        /// </summary>
        void OnMoodChanged(MoodChangedEventDto args);

        /// <summary>
        /// Called when a prediction is received.
        /// </summary>
        Task OnPredictionReceivedAsync(PredictionReceivedEventDto args);

        /// <summary>
        /// Called when a user signal is captured.
        /// </summary>
        Task OnSignalCapturedAsync(SignalCapturedEventDto args);
    }

    /// <summary>
    /// Contract for publishing events to external systems.
    /// Implement this to forward events to message queues, webhooks, etc.
    /// </summary>
    public interface IMUIBridgeEventPublisher
    {
        /// <summary>
        /// Publish theme change event.
        /// </summary>
        Task PublishThemeChangedAsync(ThemeChangedEventDto args);

        /// <summary>
        /// Publish mood change event.
        /// </summary>
        Task PublishMoodChangedAsync(MoodChangedEventDto args);

        /// <summary>
        /// Publish prediction received event.
        /// </summary>
        Task PublishPredictionReceivedAsync(PredictionReceivedEventDto args);

        /// <summary>
        /// Publish user signal event.
        /// </summary>
        Task PublishSignalCapturedAsync(SignalCapturedEventDto args);
    }

    // ============================================================================
    // WEBHOOK PAYLOAD CONTRACTS
    // ============================================================================

    /// <summary>
    /// Webhook payload for external integrations.
    /// </summary>
    public class WebhookPayloadDto
    {
        /// <summary>
        /// Webhook event type.
        /// </summary>
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Event timestamp.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Application identifier.
        /// </summary>
        [JsonPropertyName("applicationId")]
        public string? ApplicationId { get; set; }

        /// <summary>
        /// Session identifier.
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Event payload (varies by event type).
        /// </summary>
        [JsonPropertyName("payload")]
        public object? Payload { get; set; }

        /// <summary>
        /// Signature for verification (HMAC-SHA256).
        /// </summary>
        [JsonPropertyName("signature")]
        public string? Signature { get; set; }
    }

    /// <summary>
    /// Standard webhook event types.
    /// </summary>
    public static class WebhookEventTypes
    {
        public const string ThemeChanged = "theme.changed";
        public const string MoodChanged = "mood.changed";
        public const string PredictionReceived = "prediction.received";
        public const string SignalCaptured = "signal.captured";
        public const string SessionStarted = "session.started";
        public const string SessionEnded = "session.ended";
        public const string ErrorOccurred = "error.occurred";
    }
}
