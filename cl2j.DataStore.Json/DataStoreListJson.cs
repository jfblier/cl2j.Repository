using cl2j.DataStore.Core;
using cl2j.FileStorage.Core;
using cl2j.FileStorage.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace cl2j.DataStore.Json
{
    public class DataStoreListJson<TKey, TValue> : DataStoreBaseList<TKey, TValue>
    {
        private readonly IFileStorageProvider fileStorageProvider;
        private readonly string filename;
        private readonly bool indent;

        private static readonly SemaphoreSlim semaphore = new(1, 1);

        public DataStoreListJson(IFileStorageProvider fileStorageProvider, string filename, Func<TValue, TKey> getKeyPredicate, bool indent = true)
            : base(getKeyPredicate)
        {
            this.fileStorageProvider = fileStorageProvider;
            this.filename = filename;
            this.indent = indent;
        }

        public override async Task<List<TValue>> GetAllAsync()
        {
            var list = await fileStorageProvider.ReadJsonObjectAsync<List<TValue>>(filename);
            return list;
        }

        public override async Task<TValue> GetByIdAsync(TKey key)
        {
            var list = await GetAllAsync();
            return FirstOrDefault(list, key);
        }

        public override async Task InsertAsync(TValue entity)
        {
            await semaphore.WaitAsync();
            try
            {
                var list = await GetAllAsync();

                int index = FindIndex(list, entity);
                if (index >= 0)
                    throw new ConflictException();

                list.Add(entity);

                await WriteAsync(list);
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
                var list = await GetAllAsync();

                int index = FindIndex(list, entity);
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

        public override async Task DeleteAsync(TKey key)
        {
            await semaphore.WaitAsync();
            try
            {
                var list = await GetAllAsync();

                var nb = RemoveAll(list, key);
                if (nb <= 0)
                    throw new NotFoundException();

                await WriteAsync(list);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task WriteAsync(IEnumerable<TValue> list)
        {
            await fileStorageProvider.WriteJsonObjectAsync(filename, list, indent);
        }
    }
}