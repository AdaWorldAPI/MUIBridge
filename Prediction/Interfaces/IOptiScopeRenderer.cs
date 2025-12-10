// File: MUIBridge/Prediction/Interfaces/IOptiScopeRenderer.cs
// Purpose: Contract for rendering prediction/mood telemetry visually.

namespace MUIBridge.Prediction.Interfaces
{
    /// <summary>
    /// Interface for rendering prediction and mood telemetry data.
    /// Implementations can render to ASCII, HTML, or custom formats.
    /// </summary>
    public interface IOptiScopeRenderer
    {
        /// <summary>
        /// Renders the current prediction and mood state.
        /// </summary>
        /// <param name="latestPacket">Most recent prediction packet.</param>
        /// <param name="currentMood">Current UI mood state.</param>
        /// <param name="config">Rendering configuration.</param>
        /// <returns>Rendered output string.</returns>
        string RenderScope(
            PredictionPacket? latestPacket,
            UIMoodState? currentMood,
            OptiScopeConfig config);
    }

    /// <summary>
    /// Configuration for OptiScope rendering.
    /// </summary>
    public class OptiScopeConfig
    {
        /// <summary>Whether to show confidence scores.</summary>
        public bool ShowConfidence { get; set; } = true;

        /// <summary>Whether to show mood information.</summary>
        public bool ShowMood { get; set; } = true;

        /// <summary>Number of historical entries to display.</summary>
        public int HistoryLength { get; set; } = 1;

        /// <summary>Render mode: "ASCII", "HTML", etc.</summary>
        public string RenderMode { get; set; } = "ASCII";
    }
}
