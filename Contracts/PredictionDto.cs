// File: MUIBridge/Contracts/PredictionDto.cs
// Purpose: Data Transfer Objects for prediction/telemetry exchange.
// Use these DTOs when integrating with ML services or external prediction APIs.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MUIBridge.Contracts
{
    /// <summary>
    /// DTO for prediction packets from ML/AI services.
    /// Primary input for mood reactivity system.
    /// </summary>
    public class PredictionDto
    {
        /// <summary>
        /// Unique identifier for this prediction.
        /// Auto-generated if not provided.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// UTC timestamp when prediction was generated.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identifier of the ML model that generated this prediction.
        /// Examples: "UserBehaviorModel_v2", "SalesPredictor", "ChurnAnalyzer"
        /// </summary>
        [JsonPropertyName("sourceModel")]
        public string SourceModel { get; set; } = string.Empty;

        /// <summary>
        /// Context where this prediction applies.
        /// Examples: "checkout_flow", "customer_form", "dashboard"
        /// </summary>
        [JsonPropertyName("targetContext")]
        public string TargetContext { get; set; } = string.Empty;

        /// <summary>
        /// PRIMARY: Confidence score (0.0 to 1.0).
        /// This directly drives the UI mood intensity.
        /// 0.0 = low confidence/neutral, 1.0 = high confidence/engaged
        /// </summary>
        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        /// <summary>
        /// The predicted value/outcome.
        /// Examples: "will_purchase", "needs_assistance", "completing_task"
        /// </summary>
        [JsonPropertyName("predictedValue")]
        public string PredictedValue { get; set; } = string.Empty;

        /// <summary>
        /// Alternative predictions with their confidence scores.
        /// Key = prediction, Value = confidence (0.0-1.0)
        /// </summary>
        [JsonPropertyName("alternatives")]
        public Dictionary<string, float>? Alternatives { get; set; }

        /// <summary>
        /// Explicit mood intensity override (0.0 to 1.0).
        /// If provided, used instead of confidence for UI mood.
        /// </summary>
        [JsonPropertyName("moodIntensity")]
        public float? MoodIntensity { get; set; }

        /// <summary>
        /// Mood label for categorization.
        /// Standard labels: "Neutral", "Engaged", "Alert", "Optimal", "Urgent"
        /// </summary>
        [JsonPropertyName("moodLabel")]
        public string MoodLabel { get; set; } = "Neutral";

        /// <summary>
        /// Cross-reference ID for tracing through systems.
        /// Link to original signal or session.
        /// </summary>
        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Additional metadata as key-value pairs.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Time-to-live in seconds. Prediction expires after this.
        /// Default: 30 seconds
        /// </summary>
        [JsonPropertyName("ttlSeconds")]
        public int TtlSeconds { get; set; } = 30;
    }

    /// <summary>
    /// DTO for user interaction signals (telemetry input).
    /// Send these to the prediction service for analysis.
    /// </summary>
    public class UserSignalDto
    {
        /// <summary>
        /// Unique signal identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// UTC timestamp of the interaction.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Session identifier for grouping signals.
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// User identifier (anonymized if needed).
        /// </summary>
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        /// <summary>
        /// Type of action performed.
        /// Standard types: "Click", "KeyPress", "Focus", "Blur", "Scroll",
        ///                 "Hover", "Select", "Submit", "Navigate"
        /// </summary>
        [JsonPropertyName("actionType")]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the UI element interacted with.
        /// Examples: "btnSubmit", "txtCustomerName", "dgvOrders"
        /// </summary>
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = string.Empty;

        /// <summary>
        /// Type of the UI element.
        /// Examples: "Button", "TextBox", "DataGrid", "ComboBox", "Form"
        /// </summary>
        [JsonPropertyName("elementType")]
        public string ElementType { get; set; } = string.Empty;

        /// <summary>
        /// Current form/screen context.
        /// Examples: "CustomerEditForm", "OrdersView", "Dashboard"
        /// </summary>
        [JsonPropertyName("formContext")]
        public string? FormContext { get; set; }

        /// <summary>
        /// Action value (e.g., text entered, item selected).
        /// Sanitize PII before sending!
        /// </summary>
        [JsonPropertyName("actionValue")]
        public string? ActionValue { get; set; }

        /// <summary>
        /// Duration of the action in milliseconds (for Focus, Hover).
        /// </summary>
        [JsonPropertyName("durationMs")]
        public long? DurationMs { get; set; }

        /// <summary>
        /// Mouse/touch coordinates (relative to element).
        /// </summary>
        [JsonPropertyName("coordinates")]
        public PointDto? Coordinates { get; set; }

        /// <summary>
        /// Sequence number within session.
        /// Used for ordering signals.
        /// </summary>
        [JsonPropertyName("sequenceNumber")]
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Additional context as key-value pairs.
        /// </summary>
        [JsonPropertyName("context")]
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// DTO for UI mood state.
    /// Represents current mood derived from predictions.
    /// </summary>
    public class MoodStateDto
    {
        /// <summary>
        /// Context this mood applies to.
        /// "global" for application-wide, or specific form/component ID.
        /// </summary>
        [JsonPropertyName("contextId")]
        public string ContextId { get; set; } = "global";

        /// <summary>
        /// Current mood intensity (0.0 to 1.0).
        /// </summary>
        [JsonPropertyName("intensity")]
        public float Intensity { get; set; }

        /// <summary>
        /// Human-readable mood label.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; } = "Neutral";

        /// <summary>
        /// Per-component intensity breakdown (optional).
        /// Key = component ID, Value = intensity
        /// </summary>
        [JsonPropertyName("componentIntensities")]
        public Dictionary<string, float>? ComponentIntensities { get; set; }

        /// <summary>
        /// Timestamp of last update.
        /// </summary>
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Source prediction ID that caused this mood.
        /// </summary>
        [JsonPropertyName("sourcePredictionId")]
        public string? SourcePredictionId { get; set; }

        /// <summary>
        /// Trend direction: -1 = decreasing, 0 = stable, 1 = increasing
        /// </summary>
        [JsonPropertyName("trend")]
        public int Trend { get; set; } = 0;
    }

    /// <summary>
    /// Simple point DTO for coordinates.
    /// </summary>
    public class PointDto
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    /// <summary>
    /// Standard mood labels enum-like constants.
    /// </summary>
    public static class MoodLabels
    {
        public const string Neutral = "Neutral";
        public const string Low = "Low";
        public const string Engaged = "Engaged";
        public const string Alert = "Alert";
        public const string Optimal = "Optimal";
        public const string Urgent = "Urgent";
        public const string Warning = "Warning";
        public const string Success = "Success";
    }

    /// <summary>
    /// Standard action types for signals.
    /// </summary>
    public static class ActionTypes
    {
        public const string Click = "Click";
        public const string DoubleClick = "DoubleClick";
        public const string KeyPress = "KeyPress";
        public const string KeyDown = "KeyDown";
        public const string KeyUp = "KeyUp";
        public const string Focus = "Focus";
        public const string Blur = "Blur";
        public const string Scroll = "Scroll";
        public const string Hover = "Hover";
        public const string Select = "Select";
        public const string Deselect = "Deselect";
        public const string Submit = "Submit";
        public const string Cancel = "Cancel";
        public const string Navigate = "Navigate";
        public const string Resize = "Resize";
        public const string DragStart = "DragStart";
        public const string DragEnd = "DragEnd";
        public const string Drop = "Drop";
    }
}
