namespace cl2j.DataStore.Core
{
    public interface IDataStoreDictionary<TKey, TValue> where TKey : class
    {
        //Retreive all the items
        Task<Dictionary<TKey, TValue>> GetAllAsync();

        //Get an item by it's key (Id)
        Task<TValue?> GetByIdAsync(TKey key);

        //Insert a new item. The key must not exists
        Task InsertAsync(TValue entity);

        //Update an item
        Task UpdateAsync(TValue entity);

        //Delete an item
        Task DeleteAsync(TKey key);
    }
}