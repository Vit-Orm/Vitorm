using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vitorm.Sql
{
    public partial class SqlDbContext
    {

        // #0 Schema :  Create Drop Truncate
        public override async Task TryCreateTableAsync<Entity>() => await DbSet<Entity>().TryCreateTableAsync();
        public override async Task TryDropTableAsync<Entity>() => await DbSet<Entity>().TryDropTableAsync();
        public override async Task TruncateAsync<Entity>() => await DbSet<Entity>().TruncateAsync();


        // #1 Create :  Add AddRange
        public override async Task<Entity> AddAsync<Entity>(Entity entity) => await DbSet<Entity>().AddAsync(entity);
        public override async Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => await DbSet<Entity>().AddRangeAsync(entities);



        // #2 Retrieve : Get Query
        public override async Task<Entity> GetAsync<Entity>(object keyValue) => await DbSet<Entity>().GetAsync(keyValue);
        //public virtual IQueryable<Entity> Query<Entity>() => throw new NotImplementedException();


        // #3 Update: Update UpdateRange
        public override async Task<int> UpdateAsync<Entity>(Entity entity) => await DbSet<Entity>().UpdateAsync(entity);
        public override async Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => await DbSet<Entity>().UpdateRangeAsync(entities);



        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public override async Task<int> DeleteAsync<Entity>(Entity entity) => await DbSet<Entity>().DeleteAsync(entity);
        public override async Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => await DbSet<Entity>().DeleteRangeAsync(entities);

        public override async Task<int> DeleteByKeyAsync<Entity>(object keyValue) => await DbSet<Entity>().DeleteByKeyAsync(keyValue);
        public override async Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => await DbSet<Entity>().DeleteByKeysAsync<Key>(keys);



    }
}
