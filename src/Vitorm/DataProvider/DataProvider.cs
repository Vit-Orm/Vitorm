using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vitorm.DataProvider
{
    public abstract class DataProvider : IDataProvider
    {
        public abstract DbContext CreateDbContext();
        public abstract void Init(Dictionary<string, object> config);


        #region Sync Method

        // #0 Schema :  Create
        public virtual void TryCreateTable<Entity>() => CreateDbContext().TryCreateTable<Entity>();
        public virtual void TryDropTable<Entity>() => CreateDbContext().TryDropTable<Entity>();
        public virtual void Truncate<Entity>() => CreateDbContext().Truncate<Entity>();


        // #1 Create :  Add AddRange
        public virtual Entity Add<Entity>(Entity entity) => CreateDbContext().Add<Entity>(entity);
        public virtual void AddRange<Entity>(IEnumerable<Entity> entities) => CreateDbContext().AddRange<Entity>(entities);

        // #2 Retrieve : Get Query
        public virtual Entity Get<Entity>(object keyValue) => CreateDbContext().Get<Entity>(keyValue);
        public virtual IQueryable<Entity> Query<Entity>() => CreateDbContext().Query<Entity>();


        // #3 Update: Update UpdateRange
        public virtual int Update<Entity>(Entity entity) => CreateDbContext().Update<Entity>(entity);
        public virtual int UpdateRange<Entity>(IEnumerable<Entity> entities) => CreateDbContext().UpdateRange<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual int Delete<Entity>(Entity entity) => CreateDbContext().Delete<Entity>(entity);
        public virtual int DeleteRange<Entity>(IEnumerable<Entity> entities) => CreateDbContext().DeleteRange<Entity>(entities);

        public virtual int DeleteByKey<Entity>(object keyValue) => CreateDbContext().DeleteByKey<Entity>(keyValue);
        public virtual int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => CreateDbContext().DeleteByKeys<Entity, Key>(keys);

        #endregion



        #region Async Method

        // #0 Schema :  Create
        public virtual Task TryCreateTableAsync<Entity>() => CreateDbContext().TryCreateTableAsync<Entity>();
        public virtual Task TryDropTableAsync<Entity>() => CreateDbContext().TryDropTableAsync<Entity>();
        public virtual Task TruncateAsync<Entity>() => CreateDbContext().TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        public virtual Task<Entity> AddAsync<Entity>(Entity entity) => CreateDbContext().AddAsync<Entity>(entity);
        public virtual Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => CreateDbContext().AddRangeAsync<Entity>(entities);

        // #2 Retrieve : Get Query
        public virtual Task<Entity> GetAsync<Entity>(object keyValue) => CreateDbContext().GetAsync<Entity>(keyValue);



        // #3 Update: Update UpdateRange
        public virtual Task<int> UpdateAsync<Entity>(Entity entity) => CreateDbContext().UpdateAsync<Entity>(entity);
        public virtual Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => CreateDbContext().UpdateRangeAsync<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual Task<int> DeleteAsync<Entity>(Entity entity) => CreateDbContext().DeleteAsync<Entity>(entity);
        public virtual Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => CreateDbContext().DeleteRangeAsync<Entity>(entities);

        public virtual Task<int> DeleteByKeyAsync<Entity>(object keyValue) => CreateDbContext().DeleteByKeyAsync<Entity>(keyValue);
        public virtual Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => CreateDbContext().DeleteByKeysAsync<Entity, Key>(keys);

        #endregion

    }
}
