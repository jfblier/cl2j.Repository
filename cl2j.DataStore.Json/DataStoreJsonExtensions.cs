using cl2j.DataStore.Core;
using cl2j.DataStore.Core.Cache;
using cl2j.FileStorage.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace cl2j.DataStore.Json
{
    public static class DataStoreJsonExtensions
    {
        public static void AddDataStoreJsonWithCache<TKey, TValue>(this IServiceCollection services, string name, string fileStorageName, string dataStoreFileName, Func<TValue, TKey> predicate, TimeSpan refreshInterval)
        {
            services.AddSingleton<IDataStore<TKey, TValue>>(builder =>
            {
                var logger = builder.GetRequiredService<ILogger<DataStoreCache<TKey, TValue>>>();
                var fileStorageFactory = builder.GetRequiredService<IFileStorageFactory>();
                var fileStorageProvider = fileStorageFactory.Get(fileStorageName);

                var dataStore = new DataStoreJson<TKey, TValue>(fileStorageProvider, dataStoreFileName, predicate);
                var dataStoreCache = new DataStoreCache<TKey, TValue>(name, dataStore, refreshInterval, predicate, logger);

                return dataStoreCache;
            });
        }

        public static void AddDataStoreJson<TKey, TValue>(this IServiceCollection services, string fileStorageName, string dataStoreFileName, Func<TValue, TKey> predicate)
        {
            services.AddSingleton<IDataStore<TKey, TValue>>(builder =>
            {
                var fileStorageFactory = builder.GetRequiredService<IFileStorageFactory>();
                var fileStorageProvider = fileStorageFactory.Get(fileStorageName);

                var dataStore = new DataStoreJson<TKey, TValue>(fileStorageProvider, dataStoreFileName, predicate);

                return dataStore;
            });
        }
    }
}