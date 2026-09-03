using System.Collections.Concurrent;
using Tavstal.WynnNetSDK.Caching;

namespace Tavstal.DeltarBot.Services;

public class CacheService : ICacheManager, IDisposable
{
    private readonly ConcurrentDictionary<string, (object Value, DateTime ValidUntilUtc)> _cache = [];
    private readonly CancellationTokenSource _cts = new();

    public CacheService()
    {
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await UpdateAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1), _cts.Token);
                }
            }
            catch (OperationCanceledException) { /* ignored */ }
        });
    }
    
    public Task AddAsync<T>(string key, T value, DateTime validUntilUtc,
        CancellationToken cancellationToken = new())
    {
        if (_cache.ContainsKey(key) || value == null)
            return Task.CompletedTask;
        _cache[key] = (value, validUntilUtc);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = new())
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = new())
    {
        if (!_cache.TryGetValue(key, out var entry))
            return Task.FromResult<T?>(default);
        
        if (entry.Value is T value)
            return Task.FromResult(value)!;
        return Task.FromResult<T?>(default);
    }

    public async Task UpdateAsync()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in _cache.ToList())
        {
            if (now > entry.Value.ValidUntilUtc)
            {
                await RemoveAsync(entry.Key);
            }
        }
    }

    public void Dispose()
    {
        if (_cts.IsCancellationRequested)
            return;
        _cts.Cancel();
    }
}