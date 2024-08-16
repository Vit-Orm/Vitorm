using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vitorm
{
    public partial interface IDbContext
    {
        // #0 Schema :  Create Drop
        Task TryCreateTableAsync<Entity>();
        Task TryDropTableAsync<Entity>();
        Task TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        Task<Entity> AddAsync<Entity>(Entity entity);
        Task AddRangeAsync<Entity>(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        Task<Entity> GetAsync<Entity>(object keyValue);
        //IQueryable<Entity> Query<Entity>();


        // #3 Update: Update UpdateRange
        Task<int> UpdateAsync<Entity>(Entity entity);
        Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        Task<int> DeleteAsync<Entity>(Entity entity);
        Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities);

        Task<int> DeleteByKeyAsync<Entity>(object keyValue);
        Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys);
    }
}
