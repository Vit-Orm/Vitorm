using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vitorm
{
    public abstract partial class DbContext
    {

        // #0 Schema :  Create Drop Truncate
        public abstract Task TryCreateTableAsync<Entity>();
        public abstract Task TryDropTableAsync<Entity>();
        public abstract Task TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        public abstract Task<Entity> AddAsync<Entity>(Entity entity);
        public abstract Task AddRangeAsync<Entity>(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        public abstract Task<Entity> GetAsync<Entity>(object keyValue);
        //public virtual IQueryable<Entity> Query<Entity>() => throw new NotImplementedException();


        // #3 Update: Update UpdateRange
        public abstract Task<int> UpdateAsync<Entity>(Entity entity);
        public abstract Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public abstract Task<int> DeleteAsync<Entity>(Entity entity);
        public abstract Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities);


        public abstract Task<int> DeleteByKeyAsync<Entity>(object keyValue);
        public abstract Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys);



    }
}
