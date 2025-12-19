// File: MUIBridge/Cache/ICacheLayer.cs
// Purpose: Cache layer contract for 3-layer caching system (ported from DUSK).

namespace MUIBridge.Cache
{
    /// <summary>
    /// Defines the contract for a cache layer in the 3-layer cache hierarchy.
    /// </summary>
    public interface ICacheLayer
    {
        /// <summary>Name of this cache layer (e.g., "L1-Memory", "L2-Redis", "L3-MongoDB").</summary>
        string Name { get; }

        /// <summary>Priority level (lower = faster, checked first).</summary>
        int Priority { get; }

        /// <summary>Expected latency for this layer.</summary>
        TimeSpan ExpectedLatency { get; }

        /// <summary>Maximum size for this cache layer.</summary>
        long MaxSizeBytes { get; }

        /// <summary>Current size of cached data.</summary>
        long CurrentSizeBytes { get; }

        /// <summary>Gets a cached value by key.</summary>
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

        /// <summary>Sets a cached value.</summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;

        /// <summary>Removes a cached value.</summary>
        Task RemoveAsync(string key, CancellationToken ct = default);

        /// <summary>Checks if a key exists.</summary>
        Task<bool> ExistsAsync(string key, CancellationToken ct = default);

        /// <summary>Clears all cached data.</summary>
        Task ClearAsync(CancellationToken ct = default);
    }
}
