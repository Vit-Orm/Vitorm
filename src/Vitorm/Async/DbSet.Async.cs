using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vitorm
{
    public partial class DbSet<Entity> : IDbSet<Entity>
    {
        // #0 Schema :  Create Drop Truncate
        public virtual Task TryCreateTableAsync() => dbContext.TryCreateTableAsync<Entity>();
        public virtual Task TryDropTableAsync() => dbContext.TryDropTableAsync<Entity>();
        public virtual Task TruncateAsync() => dbContext.TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        public virtual Task<Entity> AddAsync(Entity entity) => dbContext.AddAsync<Entity>(entity);
        public virtual Task AddRangeAsync(IEnumerable<Entity> entities) => dbContext.AddRangeAsync<Entity>(entities);


        // #2 Retrieve : Get Query
        public virtual Task<Entity> GetAsync(object keyValue) => dbContext.GetAsync<Entity>(keyValue);


        // #3 Update: Update UpdateRange
        public virtual Task<int> UpdateAsync(Entity entity) => dbContext.UpdateAsync<Entity>(entity);
        public virtual Task<int> UpdateRangeAsync(IEnumerable<Entity> entities) => dbContext.UpdateRangeAsync<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual Task<int> DeleteAsync(Entity entity) => dbContext.DeleteAsync<Entity>(entity);
        public virtual Task<int> DeleteRangeAsync(IEnumerable<Entity> entities) => dbContext.DeleteRangeAsync<Entity>(entities);
        public virtual Task<int> DeleteByKeyAsync(object keyValue) => dbContext.DeleteByKeyAsync<Entity>(keyValue);
        public virtual Task<int> DeleteByKeysAsync<Key>(IEnumerable<Key> keys) => dbContext.DeleteByKeysAsync<Entity, Key>(keys);


    }
}
