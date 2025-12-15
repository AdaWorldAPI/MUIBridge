// File: MUIBridge/Prediction/Services/AsciiOptiScopeRenderer.cs
// Purpose: ASCII text renderer for prediction/mood telemetry.

using System.Text;
using MUIBridge.Prediction.Interfaces;

namespace MUIBridge.Prediction.Services
{
    /// <summary>
    /// Renders prediction and mood telemetry as ASCII text.
    /// Useful for debugging and console applications.
    /// </summary>
    public class AsciiOptiScopeRenderer : IOptiScopeRenderer
    {
        /// <inheritdoc />
        public string RenderScope(
            PredictionPacket? latestPacket,
            UIMoodState? currentMood,
            OptiScopeConfig config)
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════╗");
            sb.AppendLine("║        OptiScope Telemetry           ║");
            sb.AppendLine("╠══════════════════════════════════════╣");

            if (config.ShowConfidence && latestPacket != null)
            {
                sb.AppendLine($"║ Model: {latestPacket.SourceModel,-29}║");
                sb.AppendLine($"║ Prediction: {latestPacket.PredictedValue,-24}║");
                sb.AppendLine($"║ Confidence: {latestPacket.Confidence:P1,-24}║");
            }
            else if (config.ShowConfidence)
            {
                sb.AppendLine("║ Prediction: Awaiting signal...       ║");
            }

            if (config.ShowMood && currentMood != null)
            {
                sb.AppendLine("╠──────────────────────────────────────╣");
                sb.AppendLine($"║ Context: {currentMood.ContextId,-27}║");
                sb.AppendLine($"║ Mood: {currentMood.Label,-30}║");
                sb.AppendLine($"║ Intensity: {RenderBar(currentMood.Intensity),-25}║");
            }
            else if (config.ShowMood)
            {
                sb.AppendLine("╠──────────────────────────────────────╣");
                sb.AppendLine("║ Mood: Neutral                        ║");
            }

            sb.AppendLine("╚══════════════════════════════════════╝");
            return sb.ToString();
        }

        private static string RenderBar(float intensity)
        {
            int filled = (int)(intensity * 10);
            int empty = 10 - filled;
            return $"[{"█".PadRight(filled, '█')}{"░".PadRight(empty, '░')}] {intensity:P0}";
        }
    }
}
