using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vitorm
{
    public partial interface IDbSet
    {

        // #0 Schema :  Create Drop Truncate
        Task TryCreateTableAsync();
        Task TryDropTableAsync();
        Task TruncateAsync();
    }

    public partial interface IDbSet<Entity> : IDbSet
    {
        // #1 Create :  Add AddRange
        Task<Entity> AddAsync(Entity entity);
        Task AddRangeAsync(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        Task<Entity> GetAsync(object keyValue);
        //IQueryable<Entity> Query();


        // #3 Update: Update UpdateRange
        Task<int> UpdateAsync(Entity entity);
        Task<int> UpdateRangeAsync(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        Task<int> DeleteAsync(Entity entity);
        Task<int> DeleteRangeAsync(IEnumerable<Entity> entities);
        Task<int> DeleteByKeyAsync(object keyValue);
        Task<int> DeleteByKeysAsync<Key>(IEnumerable<Key> keys);


    }
}
