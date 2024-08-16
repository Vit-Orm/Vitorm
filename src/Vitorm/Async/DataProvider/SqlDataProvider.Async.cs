using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Vitorm.Sql;

namespace Vitorm.DataProvider
{
    public abstract partial class SqlDataProvider : IDataProvider
    {
        public virtual async Task<T> InvokeInDbAsync<T>(Func<SqlDbContext, Task<T>> func)
        {
            using var dbContext = CreateDbContext();
            return await func(dbContext);
        }

        public virtual async Task InvokeInDbAsync(Func<SqlDbContext, Task> func)
        {
            using var dbContext = CreateDbContext();
            await func(dbContext);
        }


        // #0 Schema :  Create Drop Truncate
        public virtual async Task TryCreateTableAsync<Entity>() => await InvokeInDbAsync(db => db.TryCreateTableAsync<Entity>());
        public virtual async Task TryDropTableAsync<Entity>() => await InvokeInDbAsync(db => db.TryDropTableAsync<Entity>());
        public virtual async Task TruncateAsync<Entity>() => await InvokeInDbAsync(db => db.TruncateAsync<Entity>());


        // #1 Create :  Add AddRange
        public virtual async Task<Entity> AddAsync<Entity>(Entity entity) => await InvokeInDbAsync(db => db.AddAsync<Entity>(entity));
        public virtual async Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => await InvokeInDbAsync(db => db.AddRangeAsync<Entity>(entities));

        // #2 Retrieve : Get Query
        public virtual async Task<Entity> GetAsync<Entity>(object keyValue) => await InvokeInDbAsync(db => db.GetAsync<Entity>(keyValue));
        //public virtual IQueryable<Entity> Query<Entity>() => CreateDbContext().AutoDisposeAfterQuery().Query<Entity>();


        // #3 Update: Update UpdateRange
        public virtual async Task<int> UpdateAsync<Entity>(Entity entity) => await InvokeInDbAsync(db => db.UpdateAsync<Entity>(entity));
        public virtual async Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => await InvokeInDbAsync(db => db.UpdateRangeAsync<Entity>(entities));


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual async Task<int> DeleteAsync<Entity>(Entity entity) => await InvokeInDbAsync(db => db.DeleteAsync<Entity>(entity));
        public virtual async Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => await InvokeInDb(db => db.DeleteRangeAsync<Entity>(entities));

        public virtual async Task<int> DeleteByKeyAsync<Entity>(object keyValue) => await InvokeInDbAsync(db => db.DeleteByKeyAsync<Entity>(keyValue));
        public virtual async Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => await InvokeInDbAsync(db => db.DeleteByKeysAsync<Entity, Key>(keys));

    }
}
