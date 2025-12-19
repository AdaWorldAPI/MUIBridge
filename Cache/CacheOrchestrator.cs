// File: MUIBridge/Cache/CacheOrchestrator.cs
// Purpose: 3-layer cache orchestrator with read-through and write-through (ported from DUSK).

using Microsoft.Extensions.Logging;

namespace MUIBridge.Cache
{
    /// <summary>
    /// Orchestrates 3-layer caching with automatic read-through (L1 -> L2 -> L3)
    /// and configurable write strategies (write-through, write-behind).
    /// </summary>
    public class CacheOrchestrator : IDisposable
    {
        private readonly List<ICacheLayer> _layers;
        private readonly ILogger<CacheOrchestrator>? _logger;
        private readonly CacheWriteStrategy _writeStrategy;
        private volatile bool _disposed;

        /// <summary>
        /// Raised on any cache operation for visualization/monitoring.
        /// </summary>
        public event EventHandler<CachePulseEventArgs>? CachePulse;

        /// <summary>
        /// Gets cache statistics.
        /// </summary>
        public CacheStatistics Statistics { get; } = new();

        public CacheOrchestrator(
            IEnumerable<ICacheLayer>? layers = null,
            CacheWriteStrategy writeStrategy = CacheWriteStrategy.WriteThrough,
            ILogger<CacheOrchestrator>? logger = null)
        {
            _layers = (layers?.OrderBy(l => l.Priority).ToList()) ?? new List<ICacheLayer>
            {
                new MemoryCacheLayer()
            };
            _writeStrategy = writeStrategy;
            _logger = logger;

            // Subscribe to pulse events from memory layer
            foreach (var layer in _layers.OfType<MemoryCacheLayer>())
            {
                layer.CachePulse += (s, e) => OnCachePulse(e);
            }

            _logger?.LogInformation("CacheOrchestrator initialized with {Count} layers: {Layers}",
                _layers.Count, string.Join(", ", _layers.Select(l => l.Name)));
        }

        /// <summary>
        /// Gets a value from cache, checking layers in priority order.
        /// Populates higher-priority layers on miss (read-through).
        /// </summary>
        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
        {
            int layerIndex = 0;

            foreach (var layer in _layers)
            {
                var value = await layer.GetAsync<T>(key, ct);

                if (value != null)
                {
                    Statistics.RecordHit(layer.Name);
                    _logger?.LogTrace("Cache hit on {Layer} for key {Key}", layer.Name, key);

                    // Populate higher-priority layers (read-through)
                    for (int i = 0; i < layerIndex; i++)
                    {
                        await _layers[i].SetAsync(key, value, ct: ct);
                    }

                    return value;
                }

                Statistics.RecordMiss(layer.Name);
                layerIndex++;
            }

            _logger?.LogTrace("Cache miss on all layers for key {Key}", key);
            return null;
        }

        /// <summary>
        /// Gets a value or creates it using the factory if not found.
        /// </summary>
        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiry = null,
            CancellationToken ct = default) where T : class
        {
            var cached = await GetAsync<T>(key, ct);
            if (cached != null) return cached;

            var value = await factory(ct);
            await SetAsync(key, value, expiry, ct);
            return value;
        }

        /// <summary>
        /// Sets a value in the cache using the configured write strategy.
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
        {
            switch (_writeStrategy)
            {
                case CacheWriteStrategy.WriteThrough:
                    // Write to all layers synchronously
                    foreach (var layer in _layers)
                    {
                        await layer.SetAsync(key, value, expiry, ct);
                    }
                    break;

                case CacheWriteStrategy.WriteToL1Only:
                    // Only write to L1 (fastest layer)
                    if (_layers.Count > 0)
                    {
                        await _layers[0].SetAsync(key, value, expiry, ct);
                    }
                    break;

                case CacheWriteStrategy.WriteBehind:
                    // Write to L1 immediately, queue others
                    if (_layers.Count > 0)
                    {
                        await _layers[0].SetAsync(key, value, expiry, ct);
                    }
                    // Queue write to other layers (fire-and-forget)
                    _ = Task.Run(async () =>
                    {
                        for (int i = 1; i < _layers.Count; i++)
                        {
                            await _layers[i].SetAsync(key, value, expiry, CancellationToken.None);
                        }
                    }, CancellationToken.None);
                    break;
            }

            OnCachePulse(new CachePulseEventArgs("Orchestrator", CachePulseType.Write, key));
        }

        /// <summary>
        /// Removes a value from all cache layers.
        /// </summary>
        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            foreach (var layer in _layers)
            {
                await layer.RemoveAsync(key, ct);
            }

            OnCachePulse(new CachePulseEventArgs("Orchestrator", CachePulseType.Invalidation, key));
        }

        /// <summary>
        /// Clears all cache layers.
        /// </summary>
        public async Task ClearAsync(CancellationToken ct = default)
        {
            foreach (var layer in _layers)
            {
                await layer.ClearAsync(ct);
            }

            _logger?.LogInformation("All cache layers cleared");
        }

        /// <summary>
        /// Gets information about all cache layers.
        /// </summary>
        public IReadOnlyList<CacheLayerInfo> GetLayerInfo()
        {
            return _layers.Select(l => new CacheLayerInfo(
                l.Name,
                l.Priority,
                l.ExpectedLatency,
                l.CurrentSizeBytes,
                l.MaxSizeBytes
            )).ToList();
        }

        private void OnCachePulse(CachePulseEventArgs e)
        {
            CachePulse?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }

    /// <summary>
    /// Write strategy for the cache orchestrator.
    /// </summary>
    public enum CacheWriteStrategy
    {
        /// <summary>Write to all layers synchronously.</summary>
        WriteThrough,

        /// <summary>Write only to L1 (fastest layer).</summary>
        WriteToL1Only,

        /// <summary>Write to L1 immediately, queue writes to other layers.</summary>
        WriteBehind
    }

    /// <summary>
    /// Information about a cache layer.
    /// </summary>
    public record CacheLayerInfo(
        string Name,
        int Priority,
        TimeSpan ExpectedLatency,
        long CurrentSizeBytes,
        long MaxSizeBytes
    );
}
