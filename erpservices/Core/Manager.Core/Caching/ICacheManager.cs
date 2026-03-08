using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Manager.Core.Caching
{
    public interface ICacheManager<T> where T : class
    {
        Task<T> GetAsync(string key);
        Task SetAsync(string key, T item, MemoryCacheEntryOptions options = null);
        Task RemoveAsync(string key);
        Task ClearAsync();
    }
} 