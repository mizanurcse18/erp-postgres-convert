using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Manager.Core.Caching
{
    public class MemoryCacheManager<T> : ICacheManager<T> where T : class
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, bool> _keys = new ConcurrentDictionary<string, bool>();

        public MemoryCacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T> GetAsync(string key)
        {
            _memoryCache.TryGetValue(key, out T item);
            return Task.FromResult(item);
        }

        public Task SetAsync(string key, T item, MemoryCacheEntryOptions options = null)
        {
            if (item == null)
            {
                return Task.CompletedTask;
            }

            _memoryCache.Set(key, item, options ?? GetDefaultCacheOptions());
            _keys.TryAdd(key, true);

            return Task.CompletedTask;
        }
        public Task ClearAsync()
        {
            foreach (var key in _keys.Keys)
            {
                _memoryCache.Remove(key);
            }
            _keys.Clear();
            return Task.CompletedTask;
        }
        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        private MemoryCacheEntryOptions GetDefaultCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
        }
    }
} 