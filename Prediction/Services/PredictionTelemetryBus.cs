// File: MUIBridge/Prediction/Services/PredictionTelemetryBus.cs
// Purpose: In-memory implementation of prediction telemetry bus.
// Can be replaced with Azure Service Bus, RabbitMQ, etc. for distributed scenarios.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MUIBridge.Prediction.Interfaces;

namespace MUIBridge.Prediction.Services
{
    /// <summary>
    /// In-memory telemetry bus for prediction signals and results.
    /// Suitable for single-process WinForms applications.
    /// </summary>
    public class PredictionTelemetryBus : IPredictionTelemetryBus
    {
        private readonly ILogger<PredictionTelemetryBus>? _logger;

        /// <inheritdoc />
        public event Func<PredictionPacket, Task>? PredictionReceivedAsync;

        /// <inheritdoc />
        public event Func<UserInteractionSignal, Task>? SignalReceivedAsync;

        public PredictionTelemetryBus(ILogger<PredictionTelemetryBus>? logger = null)
        {
            _logger = logger;
            _logger?.LogDebug("PredictionTelemetryBus initialized.");
        }

        /// <inheritdoc />
        public async Task PublishSignalAsync(UserInteractionSignal signal)
        {
            if (signal == null)
            {
                _logger?.LogWarning("Attempted to publish null signal.");
                return;
            }

            _logger?.LogTrace("Signal published: {ActionType} on {TargetElementId}",
                signal.ActionType, signal.TargetElementId);

            var handler = SignalReceivedAsync;
            if (handler != null)
            {
                try
                {
                    await handler.Invoke(signal).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error invoking SignalReceivedAsync handler.");
                }
            }
        }

        /// <inheritdoc />
        public async Task PublishPredictionAsync(PredictionPacket packet)
        {
            if (packet == null)
            {
                _logger?.LogWarning("Attempted to publish null prediction packet.");
                return;
            }

            _logger?.LogTrace("Prediction published: {PredictedValue} (Confidence: {Confidence:F2})",
                packet.PredictedValue, packet.Confidence);

            var handler = PredictionReceivedAsync;
            if (handler != null)
            {
                try
                {
                    await handler.Invoke(packet).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error invoking PredictionReceivedAsync handler.");
                }
            }
        }
    }
}
