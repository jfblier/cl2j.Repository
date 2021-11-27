using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace cl2j.DataStore.Core.Cache
{
    public class DataStoreCacheList<TKey, TValue> : DataStoreBaseList<TKey, TValue>
    {
        private readonly CacheLoader cacheLoader;
        private readonly IDataStoreList<TKey, TValue> dataStore;
        private List<TValue> cache = new();

        private static readonly SemaphoreSlim semaphore = new(1, 1);

        public DataStoreCacheList(string name, IDataStoreList<TKey, TValue> dataStore, TimeSpan refreshInterval, Func<TValue, TKey> getKeyPredicate, ILogger logger)
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

                logger.LogDebug($"DataStoreCache<{name}> --> {cache.Count} {name}(s) in {sw.ElapsedMilliseconds}ms");
            }, logger);
        }

        public override async Task<List<TValue>> GetAllAsync()
        {
            await cacheLoader.WaitAsync();
            return cache;
        }

        public override async Task<TValue> GetByIdAsync(TKey key)
        {
            await cacheLoader.WaitAsync();
            return FirstOrDefault(cache, key);
        }

        public override async Task InsertAsync(TValue entity)
        {
            await semaphore.WaitAsync();
            try
            {
                await dataStore.InsertAsync(entity);
                cache.Add(entity);
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

                var index = FindIndex(cache, entity);
                if (index >= 0)
                    cache[index] = entity;
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

                var index = FindIndex(cache, key);
                if (index >= 0)
                    cache.RemoveAt(index);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}