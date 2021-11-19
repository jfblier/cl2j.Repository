using cl2j.DataStore.Core;
using cl2j.FileStorage.Core;
using cl2j.FileStorage.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace cl2j.DataStore.Json
{
    public class DataStoreJson<TKey, TValue> : IDataStore<TKey, TValue>
    {
        private readonly IFileStorageProvider fileStorageProvider;
        private readonly string filename;
        private readonly Predicate<(TValue, TKey)> filterPredicate;
        private readonly bool indent;

        private static readonly SemaphoreSlim semaphore = new(1, 1);

        public DataStoreJson(IFileStorageProvider fileStorageProvider, string filename, Predicate<(TValue, TKey)> filterPredicate, bool indent = true)
        {
            this.fileStorageProvider = fileStorageProvider;
            this.filename = filename;
            this.filterPredicate = filterPredicate;
            this.indent = indent;
        }

        public async Task<IEnumerable<TValue>> GetAllAsync()
        {
            var list = await fileStorageProvider.ReadJsonObjectAsync<List<TValue>>(filename);
            return list;
        }

        public async Task<TValue> GetByIdAsync(TKey key)
        {
            var list = await GetAllAsync();
            return list.FirstOrDefault(item => filterPredicate((item, key)));
        }

        public async Task InsertAsync(TValue entity)
        {
            await semaphore.WaitAsync();
            try
            {
                var list = (await GetAllAsync()).ToList();

                list.Add(entity);

                await WriteAsync(list);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task UpdateAsync(TKey key, TValue entity)
        {
            await semaphore.WaitAsync();
            try
            {
                var list = (await GetAllAsync()).ToList();

                var index = list.FindIndex(item => filterPredicate((item, key)));
                if (index < 0)
                    throw new NotFoundException();

                list[index] = entity;

                await WriteAsync(list);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task DeleteAsync(TKey key)
        {
            await semaphore.WaitAsync();
            try
            {
                var list = (await GetAllAsync()).ToList();

                var nb = list.RemoveAll(item => filterPredicate((item, key)));
                if (nb <= 0)
                    throw new NotFoundException();

                await WriteAsync(list);
            }
            finally
            {
                semaphore.Release();
            }
        }

        protected async Task WriteAsync(IEnumerable<TValue> list)
        {
            await fileStorageProvider.WriteJsonObjectAsync(filename, list, indent);
        }
    }
}