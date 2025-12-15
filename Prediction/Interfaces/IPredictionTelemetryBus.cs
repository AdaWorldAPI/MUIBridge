// File: MUIBridge/Prediction/Interfaces/IPredictionTelemetryBus.cs
// Purpose: Contract for the prediction telemetry message bus.

using System;
using System.Threading.Tasks;

namespace MUIBridge.Prediction.Interfaces
{
    /// <summary>
    /// Message bus for publishing and subscribing to prediction telemetry.
    /// </summary>
    public interface IPredictionTelemetryBus
    {
        /// <summary>
        /// Publishes a user interaction signal for prediction processing.
        /// </summary>
        Task PublishSignalAsync(UserInteractionSignal signal);

        /// <summary>
        /// Publishes a prediction result from the ML layer.
        /// </summary>
        Task PublishPredictionAsync(PredictionPacket packet);

        /// <summary>
        /// Event raised when a new prediction is received.
        /// WaveMoodAdapter subscribes to this to update theme mood.
        /// </summary>
        event Func<PredictionPacket, Task>? PredictionReceivedAsync;

        /// <summary>
        /// Event raised when a new user signal is received.
        /// </summary>
        event Func<UserInteractionSignal, Task>? SignalReceivedAsync;
    }

    /// <summary>
    /// Represents a user interaction signal for telemetry and prediction.
    /// </summary>
    public class UserInteractionSignal
    {
        public Guid SignalId { get; set; } = Guid.NewGuid();
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // Click, KeyPress, Focus, Scroll
        public string TargetElementId { get; set; } = string.Empty;
        public string TargetElementType { get; set; } = string.Empty; // Button, TextBox, GridRow
        public string? ContextData { get; set; } // Optional JSON payload
        public double? DurationMs { get; set; } // For timed actions like focus
    }
}
