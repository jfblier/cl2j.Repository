using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace cl2j.DataStore.Core.Cache
{
    public class DataStoreCacheDictionary<TKey, TValue> : DataStoreBaseDictionary<TKey, TValue> where TKey : class
    {
        private readonly CacheLoader cacheLoader;
        private readonly IDataStoreDictionary<TKey, TValue> dataStore;
        private Dictionary<TKey, TValue> cache = new();

        private static readonly SemaphoreSlim semaphore = new(1, 1);

        public DataStoreCacheDictionary(string name, IDataStoreDictionary<TKey, TValue> dataStore, TimeSpan refreshInterval, Func<TValue, TKey> getKeyPredicate, ILogger logger)
            : base(getKeyPredicate)
        {
            this.dataStore = dataStore;

            cacheLoader = new CacheLoader(name, refreshInterval, async () =>
            {
                var sw = Stopwatch.StartNew();
                var tmpCache = await dataStore.GetAllAsync();

                await semaphore.WaitAsync();
                try
                {
                    cache = tmpCache;
                }
                finally
                {
                    semaphore.Release();
                }

#pragma warning disable CA2254 // Template should be a static expression
                logger.LogDebug($"DataStoreCache<{name}> --> {cache.Count} {name}(s) in {sw.ElapsedMilliseconds}ms");
#pragma warning restore CA2254 // Template should be a static expression
            }, logger);
        }

        public override async Task<Dictionary<TKey, TValue>> GetAllAsync()
        {
            await cacheLoader.WaitAsync();
            return cache;
        }

        public override async Task<TValue?> GetByIdAsync(TKey key)
        {
            await cacheLoader.WaitAsync();
            if (cache.TryGetValue(key, out var value))
                return value;

            return default;
        }

        public override async Task InsertAsync(TValue entity)
        {
            await semaphore.WaitAsync();
            try
            {
                await dataStore.InsertAsync(entity);
                Add(cache, entity);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public override async Task UpdateAsync(TValue entity)
        {
            await semaphore.WaitAsync();
            try
            {
                await dataStore.UpdateAsync(entity);
                Update(cache, entity);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public override async Task DeleteAsync(TKey key)
        {
            await semaphore.WaitAsync();
            try
            {
                await dataStore.DeleteAsync(key);
                cache.Remove(key);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}