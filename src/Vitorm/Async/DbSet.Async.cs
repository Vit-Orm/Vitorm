using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vitorm
{


    public partial class DbSet<Entity> : IDbSet<Entity>
    {
        // #0 Schema :  Create Drop Truncate
        public virtual async Task TryCreateTableAsync() => await dbContext.TryCreateTableAsync<Entity>();
        public virtual async Task TryDropTableAsync() => await dbContext.TryDropTableAsync<Entity>();
        public virtual async Task TruncateAsync() => await dbContext.TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        public virtual async Task<Entity> AddAsync(Entity entity) => await dbContext.AddAsync<Entity>(entity);
        public virtual async Task AddRangeAsync(IEnumerable<Entity> entities) => await dbContext.AddRangeAsync<Entity>(entities);


        // #2 Retrieve : Get Query
        public virtual async Task<Entity> GetAsync(object keyValue) => await dbContext.GetAsync<Entity>(keyValue);
        //public virtual IQueryable<Entity> Query() => dbContext.Query<Entity>();


        // #3 Update: Update UpdateRange
        public virtual async Task<int> UpdateAsync(Entity entity) => await dbContext.UpdateAsync<Entity>(entity);
        public virtual async Task<int> UpdateRangeAsync(IEnumerable<Entity> entities) => await dbContext.UpdateRangeAsync<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual async Task<int> DeleteAsync(Entity entity) => await dbContext.DeleteAsync<Entity>(entity);
        public virtual async Task<int> DeleteRangeAsync(IEnumerable<Entity> entities) => await dbContext.DeleteRangeAsync<Entity>(entities);
        public virtual async Task<int> DeleteByKeyAsync(object keyValue) => await dbContext.DeleteByKeyAsync<Entity>(keyValue);
        public virtual async Task<int> DeleteByKeysAsync<Key>(IEnumerable<Key> keys) => await dbContext.DeleteByKeysAsync<Entity, Key>(keys);


    }
}
