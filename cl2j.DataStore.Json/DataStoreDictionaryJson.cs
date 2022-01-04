using cl2j.DataStore.Core;
using cl2j.FileStorage.Core;
using cl2j.FileStorage.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace cl2j.DataStore.Json
{
    public class DataStoreDictionaryJson<TKey, TValue> : DataStoreBaseDictionary<TKey, TValue> where TKey : class
    {
        private readonly IFileStorageProvider fileStorageProvider;
        private readonly string filename;
        private readonly bool indent;

        private static readonly SemaphoreSlim semaphore = new(1, 1);

        public DataStoreDictionaryJson(IFileStorageProvider fileStorageProvider, string filename, Func<TValue, TKey> getKeyPredicate, bool indent = true)
            : base(getKeyPredicate)
        {
            this.fileStorageProvider = fileStorageProvider;
            this.filename = filename;
            this.indent = indent;
        }

        public override async Task<Dictionary<TKey, TValue>> GetAllAsync()
        {
            var dict = await fileStorageProvider.ReadJsonObjectAsync<Dictionary<TKey, TValue>>(filename);
            return dict;
        }

        public override async Task<TValue> GetByIdAsync(TKey key)
        {
            var dict = await GetAllAsync();
            if (dict.TryGetValue(key, out var value))
                return value;
            return default;
        }

        public override async Task InsertAsync(TValue entity)
        {
            await semaphore.WaitAsync();
            try
            {
                var dict = await GetAllAsync();
                Add(dict, entity);
                await WriteAsync(dict);
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
                var dict = await GetAllAsync();

                Update(dict, entity);

                await WriteAsync(dict);
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
                var dict = await GetAllAsync();

                if (!dict.Remove(key))
                    throw new NotFoundException();

                await WriteAsync(dict);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task WriteAsync(IDictionary<TKey, TValue> list)
        {
            await fileStorageProvider.WriteJsonObjectAsync(filename, list, indent);
        }
    }
}