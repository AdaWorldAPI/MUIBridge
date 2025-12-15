// File: MUIBridge/Prediction/Services/WaveMoodAdapter.cs
// Purpose: Bridges prediction telemetry to ThemeManager mood state.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MUIBridge.Core;
using MUIBridge.Prediction.Interfaces;

namespace MUIBridge.Prediction.Services
{
    /// <summary>
    /// Adapts prediction confidence scores from the telemetry bus into
    /// mood intensity updates for the ThemeManager.
    /// </summary>
    public class WaveMoodAdapter : IWaveMoodAdapter
    {
        private readonly IPredictionTelemetryBus _telemetryBus;
        private readonly ThemeManager _themeManager;
        private readonly ILogger<WaveMoodAdapter>? _logger;

        private float _lastAppliedMoodIntensity = -1f;
        private readonly float _minDeltaThreshold = 0.01f;
        private Func<PredictionPacket, Task>? _predictionHandler;
        private UIMoodState _currentMoodState;
        private bool _isRunning;

        /// <inheritdoc />
        public event EventHandler<UIMoodState>? MoodChanged;

        public WaveMoodAdapter(
            IPredictionTelemetryBus telemetryBus,
            ThemeManager themeManager,
            ILogger<WaveMoodAdapter>? logger = null)
        {
            _telemetryBus = telemetryBus ?? throw new ArgumentNullException(nameof(telemetryBus));
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger;

            _currentMoodState = new UIMoodState
            {
                ContextId = "global",
                Intensity = 0f,
                Label = "Neutral"
            };

            _logger?.LogInformation("WaveMoodAdapter initialized.");
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                _logger?.LogDebug("WaveMoodAdapter already running.");
                return Task.CompletedTask;
            }

            _logger?.LogInformation("Starting WaveMoodAdapter...");

            _predictionHandler = HandlePredictionPacketAsync;
            _telemetryBus.PredictionReceivedAsync += _predictionHandler;
            _isRunning = true;

            _logger?.LogInformation("WaveMoodAdapter subscribed to telemetry bus.");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!_isRunning)
            {
                return Task.CompletedTask;
            }

            _logger?.LogInformation("Stopping WaveMoodAdapter...");

            if (_predictionHandler != null)
            {
                _telemetryBus.PredictionReceivedAsync -= _predictionHandler;
                _predictionHandler = null;
            }

            _isRunning = false;
            _logger?.LogInformation("WaveMoodAdapter stopped.");
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<UIMoodState> GetCurrentMoodAsync(string uiContextId)
        {
            // For now, return global mood state
            // Future: support per-context mood states
            return Task.FromResult(_currentMoodState);
        }

        /// <inheritdoc />
        public Task UpdateMoodFromPredictionAsync(PredictionPacket packet)
        {
            return HandlePredictionPacketAsync(packet);
        }

        private Task HandlePredictionPacketAsync(PredictionPacket packet)
        {
            if (packet == null) return Task.CompletedTask;

            _logger?.LogTrace("Processing prediction: Confidence={Confidence:F2}, CXRef={CXRef}",
                packet.Confidence, packet.CXRef);

            float newMoodIntensity = packet.Confidence;

            // Apply minimum delta threshold to prevent UI flicker
            if (Math.Abs(newMoodIntensity - _lastAppliedMoodIntensity) < _minDeltaThreshold
                && _lastAppliedMoodIntensity >= 0)
            {
                _logger?.LogTrace("Mood change below threshold, skipping update.");
                return Task.CompletedTask;
            }

            // Update ThemeManager
            try
            {
                _themeManager.SetPredictionMood(newMoodIntensity);
                _lastAppliedMoodIntensity = newMoodIntensity;

                // Update local mood state
                _currentMoodState = new UIMoodState
                {
                    ContextId = packet.TargetContext ?? "global",
                    Intensity = newMoodIntensity,
                    Label = GetMoodLabel(newMoodIntensity),
                    LastUpdated = DateTime.UtcNow
                };

                // Raise event for any direct subscribers
                MoodChanged?.Invoke(this, _currentMoodState);

                _logger?.LogDebug("Mood updated to {Intensity:F2} ({Label})",
                    _currentMoodState.Intensity, _currentMoodState.Label);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating mood in ThemeManager.");
            }

            return Task.CompletedTask;
        }

        private static string GetMoodLabel(float intensity) => intensity switch
        {
            < 0.25f => "Neutral",
            < 0.50f => "Engaged",
            < 0.75f => "Alert",
            _ => "Optimal"
        };

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            _logger?.LogDebug("WaveMoodAdapter disposed.");
            GC.SuppressFinalize(this);
        }
    }
}
