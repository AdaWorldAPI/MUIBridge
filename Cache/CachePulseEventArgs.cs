// File: MUIBridge/Cache/CachePulseEventArgs.cs
// Purpose: Event args for cache pulse visualization (ported from DUSK).

namespace MUIBridge.Cache
{
    /// <summary>
    /// Types of cache pulse events for visualization.
    /// </summary>
    public enum CachePulseType
    {
        Hit,
        Miss,
        Write,
        Invalidation
    }

    /// <summary>
    /// Event args for cache operations, used for Winamp-style waveform visualization.
    /// </summary>
    public class CachePulseEventArgs : EventArgs
    {
        /// <summary>Name of the cache layer that generated the event.</summary>
        public string LayerName { get; }

        /// <summary>Type of cache operation.</summary>
        public CachePulseType PulseType { get; }

        /// <summary>Cache key involved.</summary>
        public string Key { get; }

        /// <summary>Timestamp of the event.</summary>
        public DateTime Timestamp { get; }

        public CachePulseEventArgs(string layerName, CachePulseType pulseType, string key)
        {
            LayerName = layerName;
            PulseType = pulseType;
            Key = key;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Statistics for cache operations.
    /// </summary>
    public class CacheStatistics
    {
        private readonly Dictionary<string, LayerStats> _layerStats = new();
        private readonly object _lock = new();

        public void RecordHit(string layerName)
        {
            lock (_lock)
            {
                GetOrCreateStats(layerName).Hits++;
            }
        }

        public void RecordMiss(string layerName)
        {
            lock (_lock)
            {
                GetOrCreateStats(layerName).Misses++;
            }
        }

        public double GetHitRate(string layerName)
        {
            lock (_lock)
            {
                if (!_layerStats.TryGetValue(layerName, out var stats))
                    return 0;

                var total = stats.Hits + stats.Misses;
                return total == 0 ? 0 : (double)stats.Hits / total;
            }
        }

        public IReadOnlyDictionary<string, (long Hits, long Misses, double HitRate)> GetAllStats()
        {
            lock (_lock)
            {
                return _layerStats.ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                    {
                        var total = kvp.Value.Hits + kvp.Value.Misses;
                        var hitRate = total == 0 ? 0 : (double)kvp.Value.Hits / total;
                        return (kvp.Value.Hits, kvp.Value.Misses, hitRate);
                    }
                );
            }
        }

        private LayerStats GetOrCreateStats(string layerName)
        {
            if (!_layerStats.TryGetValue(layerName, out var stats))
            {
                stats = new LayerStats();
                _layerStats[layerName] = stats;
            }
            return stats;
        }

        private class LayerStats
        {
            public long Hits;
            public long Misses;
        }
    }
}
