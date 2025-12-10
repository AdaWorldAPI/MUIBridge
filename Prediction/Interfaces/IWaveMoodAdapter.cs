// File: MUIBridge/Prediction/Interfaces/IWaveMoodAdapter.cs
// Purpose: Contract for adapting prediction telemetry to UI mood state.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace MUIBridge.Prediction.Interfaces
{
    /// <summary>
    /// Adapts prediction confidence scores into UI mood state updates.
    /// Bridges the gap between AI predictions and visual feedback.
    /// </summary>
    public interface IWaveMoodAdapter : IDisposable
    {
        /// <summary>
        /// Starts listening to prediction telemetry and updating mood state.
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops listening to prediction telemetry.
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current mood state for a specific UI context.
        /// </summary>
        /// <param name="uiContextId">The UI context identifier.</param>
        /// <returns>Current mood state.</returns>
        Task<UIMoodState> GetCurrentMoodAsync(string uiContextId);

        /// <summary>
        /// Manually update mood from a prediction packet (for direct integration).
        /// </summary>
        Task UpdateMoodFromPredictionAsync(PredictionPacket packet);

        /// <summary>
        /// Event raised when mood state changes.
        /// </summary>
        event EventHandler<UIMoodState>? MoodChanged;
    }

    /// <summary>
    /// Represents the current UI mood state derived from predictions.
    /// </summary>
    public class UIMoodState
    {
        /// <summary>UI context this mood applies to.</summary>
        public string ContextId { get; set; } = string.Empty;

        /// <summary>Combined mood intensity (0.0 to 1.0).</summary>
        public float Intensity { get; set; }

        /// <summary>Human-readable mood label (e.g., "Neutral", "Alert", "Optimal").</summary>
        public string Label { get; set; } = "Neutral";

        /// <summary>Optional per-component intensity breakdown.</summary>
        public Dictionary<string, float>? ComponentIntensities { get; set; }

        /// <summary>Timestamp of last update.</summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Prediction packet from the AI/ML layer.
    /// </summary>
    public class PredictionPacket
    {
        public Guid PacketId { get; set; } = Guid.NewGuid();
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string SourceModel { get; set; } = string.Empty;
        public string TargetContext { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public string PredictedValue { get; set; } = string.Empty;
        public Dictionary<string, float>? Alternatives { get; set; }
        public float MoodIntensity { get; set; }
        public string MoodLabel { get; set; } = "Neutral";
        public string? CXRef { get; set; } // Cross-reference ID for tracing
    }
}
