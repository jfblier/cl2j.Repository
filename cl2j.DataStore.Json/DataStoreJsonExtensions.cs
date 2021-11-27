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
        #region List

        public static void AddDataStoreListJsonWithCache<TKey, TValue>(this IServiceCollection services, string fileStorageName, string dataStoreFileName, Func<TValue, TKey> predicate, TimeSpan refreshInterval)
        {
            services.AddSingleton<IDataStoreList<TKey, TValue>>(builder =>
            {
                var logger = builder.GetRequiredService<ILogger<DataStoreCacheList<TKey, TValue>>>();
                var fileStorageFactory = builder.GetRequiredService<IFileStorageFactory>();
                var fileStorageProvider = fileStorageFactory.Get(fileStorageName);

                var dataStore = new DataStoreListJson<TKey, TValue>(fileStorageProvider, dataStoreFileName, predicate);
                var dataStoreCache = new DataStoreCacheList<TKey, TValue>(GetName<TValue>(), dataStore, refreshInterval, predicate, logger);

                return dataStoreCache;
            });
        }

        public static void AddDataStoreListJson<TKey, TValue>(this IServiceCollection services, string fileStorageName, string dataStoreFileName, Func<TValue, TKey> predicate)
        {
            services.AddSingleton<IDataStoreList<TKey, TValue>>(builder =>
            {
                var fileStorageFactory = builder.GetRequiredService<IFileStorageFactory>();
                var fileStorageProvider = fileStorageFactory.Get(fileStorageName);

                var dataStore = new DataStoreListJson<TKey, TValue>(fileStorageProvider, dataStoreFileName, predicate);

                return dataStore;
            });
        }

        #endregion List

        #region Dictionary

        public static void AddDataStoreDictionaryJsonWithCache<TKey, TValue>(this IServiceCollection services, string fileStorageName, string dataStoreFileName, Func<TValue, TKey> predicate, TimeSpan refreshInterval)
        {
            services.AddSingleton<IDataStoreDictionary<TKey, TValue>>(builder =>
            {
                var logger = builder.GetRequiredService<ILogger<DataStoreCacheDictionary<TKey, TValue>>>();
                var fileStorageFactory = builder.GetRequiredService<IFileStorageFactory>();
                var fileStorageProvider = fileStorageFactory.Get(fileStorageName);

                var dataStore = new DataStoreDictionaryJson<TKey, TValue>(fileStorageProvider, dataStoreFileName, predicate);
                var dataStoreCache = new DataStoreCacheDictionary<TKey, TValue>(GetName<TValue>(), dataStore, refreshInterval, predicate, logger);

                return dataStoreCache;
            });
        }

        public static void AddDataStoreDictionaryJson<TKey, TValue>(this IServiceCollection services, string fileStorageName, string dataStoreFileName, Func<TValue, TKey> predicate)
        {
            services.AddSingleton<IDataStoreDictionary<TKey, TValue>>(builder =>
            {
                var fileStorageFactory = builder.GetRequiredService<IFileStorageFactory>();
                var fileStorageProvider = fileStorageFactory.Get(fileStorageName);

                var dataStore = new DataStoreDictionaryJson<TKey, TValue>(fileStorageProvider, dataStoreFileName, predicate);

                return dataStore;
            });
        }

        #endregion Dictionary

        #region Private

        private static string GetName<T>()
        {
            return typeof(T).Name.ToLower();
        }

        #endregion Private
    }
}