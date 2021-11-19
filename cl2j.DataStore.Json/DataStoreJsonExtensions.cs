using cl2j.DataStore.Core;
using cl2j.FileStorage.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace cl2j.DataStore.Json
{
    public static class DataStoreJsonExtensions
    {
        public static void AddDataStoreJson<TKey, TValue>(this IServiceCollection services, string fileStorageName, string dataStoreFileName, Predicate<(TValue, TKey)> predicate)
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