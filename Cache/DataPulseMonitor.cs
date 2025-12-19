// File: MUIBridge/Cache/DataPulseMonitor.cs
// Purpose: Winamp-style waveform data for cache "breathing" visualization (ported from DUSK).

namespace MUIBridge.Cache
{
    /// <summary>
    /// Monitors cache activity and generates Winamp-style waveform data
    /// for visualizing database "breathing" patterns.
    /// </summary>
    public class DataPulseMonitor : IDisposable
    {
        private readonly int _bufferSize;
        private readonly float[] _l1Waveform;
        private readonly float[] _l2Waveform;
        private readonly float[] _l3Waveform;
        private readonly object _lock = new();
        private int _writeIndex;
        private volatile bool _disposed;

        private float _l1Activity;
        private float _l2Activity;
        private float _l3Activity;
        private int _eventsThisSecond;
        private DateTime _lastSecond = DateTime.UtcNow;

        /// <summary>Current L1 (Memory) activity level (0.0 to 1.0).</summary>
        public float L1Activity => _l1Activity;

        /// <summary>Current L2 (Redis) activity level (0.0 to 1.0).</summary>
        public float L2Activity => _l2Activity;

        /// <summary>Current L3 (MongoDB) activity level (0.0 to 1.0).</summary>
        public float L3Activity => _l3Activity;

        /// <summary>Cache events per second.</summary>
        public int EventsPerSecond { get; private set; }

        /// <summary>Waveform buffer for L1 visualization.</summary>
        public IReadOnlyList<float> L1Waveform => _l1Waveform;

        /// <summary>Waveform buffer for L2 visualization.</summary>
        public IReadOnlyList<float> L2Waveform => _l2Waveform;

        /// <summary>Waveform buffer for L3 visualization.</summary>
        public IReadOnlyList<float> L3Waveform => _l3Waveform;

        /// <summary>
        /// Creates a new DataPulseMonitor.
        /// </summary>
        /// <param name="bufferSize">Size of the waveform buffer (samples).</param>
        public DataPulseMonitor(int bufferSize = 256)
        {
            _bufferSize = bufferSize;
            _l1Waveform = new float[bufferSize];
            _l2Waveform = new float[bufferSize];
            _l3Waveform = new float[bufferSize];
        }

        /// <summary>
        /// Connects to a CacheOrchestrator to receive pulse events.
        /// </summary>
        public void Connect(CacheOrchestrator orchestrator)
        {
            orchestrator.CachePulse += OnCachePulse;
        }

        /// <summary>
        /// Disconnects from a CacheOrchestrator.
        /// </summary>
        public void Disconnect(CacheOrchestrator orchestrator)
        {
            orchestrator.CachePulse -= OnCachePulse;
        }

        /// <summary>
        /// Call this every frame to update waveform decay and statistics.
        /// </summary>
        /// <param name="deltaTime">Time since last update in seconds.</param>
        public void Update(float deltaTime)
        {
            if (_disposed) return;

            // Decay activity levels
            const float decayRate = 3f;
            _l1Activity = Math.Max(0, _l1Activity - decayRate * deltaTime);
            _l2Activity = Math.Max(0, _l2Activity - decayRate * deltaTime);
            _l3Activity = Math.Max(0, _l3Activity - decayRate * deltaTime);

            // Update waveform buffers
            lock (_lock)
            {
                _l1Waveform[_writeIndex] = _l1Activity;
                _l2Waveform[_writeIndex] = _l2Activity;
                _l3Waveform[_writeIndex] = _l3Activity;
                _writeIndex = (_writeIndex + 1) % _bufferSize;
            }

            // Update events per second
            var now = DateTime.UtcNow;
            if ((now - _lastSecond).TotalSeconds >= 1.0)
            {
                EventsPerSecond = _eventsThisSecond;
                _eventsThisSecond = 0;
                _lastSecond = now;
            }
        }

        private void OnCachePulse(object? sender, CachePulseEventArgs e)
        {
            _eventsThisSecond++;

            // Map layer name to activity level
            float pulseIntensity = e.PulseType switch
            {
                CachePulseType.Hit => 0.8f,
                CachePulseType.Miss => 0.3f,
                CachePulseType.Write => 1.0f,
                CachePulseType.Invalidation => 0.5f,
                _ => 0.5f
            };

            if (e.LayerName.Contains("L1") || e.LayerName.Contains("Memory"))
            {
                _l1Activity = Math.Min(1f, _l1Activity + pulseIntensity);
            }
            else if (e.LayerName.Contains("L2") || e.LayerName.Contains("Redis"))
            {
                _l2Activity = Math.Min(1f, _l2Activity + pulseIntensity);
            }
            else if (e.LayerName.Contains("L3") || e.LayerName.Contains("Mongo"))
            {
                _l3Activity = Math.Min(1f, _l3Activity + pulseIntensity);
            }
        }

        /// <summary>
        /// Gets the waveform data as a contiguous array starting from the oldest sample.
        /// </summary>
        public float[] GetOrderedWaveform(int layer)
        {
            var source = layer switch
            {
                1 => _l1Waveform,
                2 => _l2Waveform,
                3 => _l3Waveform,
                _ => _l1Waveform
            };

            lock (_lock)
            {
                var result = new float[_bufferSize];
                for (int i = 0; i < _bufferSize; i++)
                {
                    result[i] = source[(_writeIndex + i) % _bufferSize];
                }
                return result;
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
