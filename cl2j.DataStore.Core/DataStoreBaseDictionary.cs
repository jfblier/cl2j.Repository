namespace cl2j.DataStore.Core
{
    public abstract class DataStoreBaseDictionary<TKey, TValue> : IDataStoreDictionary<TKey, TValue> where TKey : class
    {
        private readonly Func<TValue, TKey> getKeyPredicate;

        public DataStoreBaseDictionary(Func<TValue, TKey> getKeyPredicate)
        {
            this.getKeyPredicate = getKeyPredicate;
        }

        public abstract Task DeleteAsync(TKey key);

        public abstract Task<Dictionary<TKey, TValue>> GetAllAsync();

        public abstract Task<TValue?> GetByIdAsync(TKey key);

        public abstract Task InsertAsync(TValue entity);

        public abstract Task UpdateAsync(TValue entity);

        protected void Add(Dictionary<TKey, TValue> dict, TValue entity)
        {
            var key = getKeyPredicate(entity);

            if (dict.ContainsKey(key))
                throw new ConflictException();

            dict.Add(key, entity);
        }

        protected void Update(Dictionary<TKey, TValue> dict, TValue entity)
        {
            var key = getKeyPredicate(entity);
            dict[key] = entity;
        }
    }
}