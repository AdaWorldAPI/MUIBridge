// File: MUIBridge/Prediction/Interfaces/IRubiconGate.cs
// Purpose: Contract for the prediction engine (ML model interface).

using System.Threading;
using System.Threading.Tasks;

namespace MUIBridge.Prediction.Interfaces
{
    /// <summary>
    /// Interface for the core prediction engine.
    /// Implementations call ML models, apply rules, and generate prediction packets.
    /// </summary>
    public interface IRubiconGate
    {
        /// <summary>
        /// Generates a prediction based on a user interaction signal.
        /// </summary>
        /// <param name="contextSignal">The user interaction context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Prediction packet with confidence score and mood info.</returns>
        Task<PredictionPacket> GetPredictionAsync(
            UserInteractionSignal contextSignal,
            CancellationToken cancellationToken = default);
    }
}
