using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vitorm.Sql
{
    public partial class SqlDbContext
    {

        // #0 Schema :  Create Drop Truncate
        public override Task TryCreateTableAsync<Entity>() => DbSet<Entity>().TryCreateTableAsync();
        public override Task TryDropTableAsync<Entity>() => DbSet<Entity>().TryDropTableAsync();
        public override Task TruncateAsync<Entity>() => DbSet<Entity>().TruncateAsync();


        // #1 Create :  Add AddRange
        public override Task<Entity> AddAsync<Entity>(Entity entity) => DbSet<Entity>().AddAsync(entity);
        public override Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().AddRangeAsync(entities);



        // #2 Retrieve : Get Query
        public override Task<Entity> GetAsync<Entity>(object keyValue) => DbSet<Entity>().GetAsync(keyValue);


        // #3 Update: Update UpdateRange
        public override Task<int> UpdateAsync<Entity>(Entity entity) => DbSet<Entity>().UpdateAsync(entity);
        public override Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().UpdateRangeAsync(entities);



        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public override Task<int> DeleteAsync<Entity>(Entity entity) => DbSet<Entity>().DeleteAsync(entity);
        public override Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().DeleteRangeAsync(entities);

        public override Task<int> DeleteByKeyAsync<Entity>(object keyValue) => DbSet<Entity>().DeleteByKeyAsync(keyValue);
        public override Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => DbSet<Entity>().DeleteByKeysAsync<Key>(keys);



    }
}
