namespace EnterpriseCore.Application.Interfaces;

/// <summary>
/// Service for caching operations using Redis or other distributed cache providers.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value by its key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The cached value if found, or null if the key does not exist.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value in the cache with an optional expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="expiration">Optional expiration time for the cached value. If null, the default expiration is used.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value by its key.
    /// </summary>
    /// <param name="key">The unique cache key to remove.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values with keys that start with the specified prefix.
    /// </summary>
    /// <param name="prefix">The key prefix to match for removal.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a value exists in the cache for the specified key.
    /// </summary>
    /// <param name="key">The unique cache key to check.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the key exists in the cache, false otherwise.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a cached value by its key, or generates and caches it using the provided factory if not found.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <param name="factory">A function that generates the value if it is not found in the cache.</param>
    /// <param name="expiration">Optional expiration time for the cached value. If null, the default expiration is used.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The cached value if found, or the newly generated and cached value.</returns>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
}
