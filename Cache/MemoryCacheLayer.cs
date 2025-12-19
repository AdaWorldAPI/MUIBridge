// File: MUIBridge/Cache/MemoryCacheLayer.cs
// Purpose: L1 in-memory cache layer with LRU eviction (ported from DUSK).

using System.Collections.Concurrent;

namespace MUIBridge.Cache
{
    /// <summary>
    /// L1 in-memory cache layer. Fastest tier with sub-millisecond access.
    /// Uses ConcurrentDictionary for thread-safety with LRU eviction.
    /// </summary>
    public class MemoryCacheLayer : ICacheLayer
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly long _maxSizeBytes;
        private long _currentSizeBytes;

        public string Name => "L1-Memory";
        public int Priority => 1;
        public TimeSpan ExpectedLatency => TimeSpan.FromMilliseconds(0.1);
        public long MaxSizeBytes => _maxSizeBytes;
        public long CurrentSizeBytes => Interlocked.Read(ref _currentSizeBytes);

        /// <summary>
        /// Raised when a cache hit or miss occurs.
        /// </summary>
        public event EventHandler<CachePulseEventArgs>? CachePulse;

        public MemoryCacheLayer(long maxSizeBytes = 100 * 1024 * 1024) // 100 MB default
        {
            _maxSizeBytes = maxSizeBytes;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.Expiry == null || entry.Expiry > DateTime.UtcNow)
                {
                    entry.LastAccessed = DateTime.UtcNow;
                    CachePulse?.Invoke(this, new CachePulseEventArgs(Name, CachePulseType.Hit, key));
                    return Task.FromResult(entry.Value as T);
                }

                // Expired - remove it
                _cache.TryRemove(key, out _);
            }

            CachePulse?.Invoke(this, new CachePulseEventArgs(Name, CachePulseType.Miss, key));
            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
        {
            var expiryTime = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : (DateTime?)null;
            var entry = new CacheEntry(value, expiryTime);

            // Estimate size (rough approximation)
            long estimatedSize = EstimateSize(value);

            // Evict if needed
            while (CurrentSizeBytes + estimatedSize > _maxSizeBytes && _cache.Count > 0)
            {
                EvictLru();
            }

            _cache.AddOrUpdate(key, entry, (_, _) => entry);
            Interlocked.Add(ref _currentSizeBytes, estimatedSize);

            CachePulse?.Invoke(this, new CachePulseEventArgs(Name, CachePulseType.Write, key));
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            if (_cache.TryRemove(key, out var entry))
            {
                Interlocked.Add(ref _currentSizeBytes, -EstimateSize(entry.Value));
                CachePulse?.Invoke(this, new CachePulseEventArgs(Name, CachePulseType.Invalidation, key));
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                return Task.FromResult(entry.Expiry == null || entry.Expiry > DateTime.UtcNow);
            }
            return Task.FromResult(false);
        }

        public Task ClearAsync(CancellationToken ct = default)
        {
            _cache.Clear();
            Interlocked.Exchange(ref _currentSizeBytes, 0);
            return Task.CompletedTask;
        }

        private void EvictLru()
        {
            var oldest = _cache
                .OrderBy(kvp => kvp.Value.LastAccessed)
                .FirstOrDefault();

            if (oldest.Key != null && _cache.TryRemove(oldest.Key, out var entry))
            {
                Interlocked.Add(ref _currentSizeBytes, -EstimateSize(entry.Value));
            }
        }

        private static long EstimateSize(object value)
        {
            // Rough estimate: 100 bytes base + string length if string
            return value switch
            {
                string s => 100 + s.Length * 2,
                _ => 500 // Assume ~500 bytes for objects
            };
        }

        private class CacheEntry
        {
            public object Value { get; }
            public DateTime? Expiry { get; }
            public DateTime LastAccessed { get; set; }

            public CacheEntry(object value, DateTime? expiry)
            {
                Value = value;
                Expiry = expiry;
                LastAccessed = DateTime.UtcNow;
            }
        }
    }
}
