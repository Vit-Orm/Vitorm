using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Vitorm.Sql;

namespace Vitorm.DataProvider
{
    public abstract partial class SqlDataProvider : IDataProvider
    {
        DbContext IDataProvider.CreateDbContext() => this.CreateDbContext();

        public abstract SqlDbContext CreateDbContext();

        public abstract void Init(Dictionary<string, object> config);



        #region Sync Method

        public virtual T InvokeInDb<T>(Func<SqlDbContext, T> func)
        {
            using var dbContext = CreateDbContext();
            return func(dbContext);
        }

        public virtual void InvokeInDb(Action<SqlDbContext> func)
        {
            using var dbContext = CreateDbContext();
            func(dbContext);
        }


        // #0 Schema :  Create
        public virtual void TryCreateTable<Entity>() => InvokeInDb(db => db.TryCreateTable<Entity>());
        public virtual void TryDropTable<Entity>() => InvokeInDb(db => db.TryDropTable<Entity>());
        public virtual void Truncate<Entity>() => InvokeInDb(db => db.Truncate<Entity>());


        // #1 Create :  Add AddRange
        public virtual Entity Add<Entity>(Entity entity) => InvokeInDb(db => db.Add<Entity>(entity));
        public virtual void AddRange<Entity>(IEnumerable<Entity> entities) => InvokeInDb(db => db.AddRange<Entity>(entities));

        // #2 Retrieve : Get Query
        public virtual Entity Get<Entity>(object keyValue) => InvokeInDb(db => db.Get<Entity>(keyValue));
        public virtual IQueryable<Entity> Query<Entity>() => CreateDbContext().AutoDisposeAfterQuery().Query<Entity>();


        // #3 Update: Update UpdateRange
        public virtual int Update<Entity>(Entity entity) => InvokeInDb(db => db.Update<Entity>(entity));
        public virtual int UpdateRange<Entity>(IEnumerable<Entity> entities) => InvokeInDb(db => db.UpdateRange<Entity>(entities));


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual int Delete<Entity>(Entity entity) => InvokeInDb(db => db.Delete<Entity>(entity));
        public virtual int DeleteRange<Entity>(IEnumerable<Entity> entities) => InvokeInDb(db => db.DeleteRange<Entity>(entities));

        public virtual int DeleteByKey<Entity>(object keyValue) => InvokeInDb(db => db.DeleteByKey<Entity>(keyValue));
        public virtual int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => InvokeInDb(db => db.DeleteByKeys<Entity, Key>(keys));





        #endregion



        #region Async Method

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


        // #3 Update: Update UpdateRange
        public virtual async Task<int> UpdateAsync<Entity>(Entity entity) => await InvokeInDbAsync(db => db.UpdateAsync<Entity>(entity));
        public virtual async Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => await InvokeInDbAsync(db => db.UpdateRangeAsync<Entity>(entities));


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual async Task<int> DeleteAsync<Entity>(Entity entity) => await InvokeInDbAsync(db => db.DeleteAsync<Entity>(entity));
        public virtual async Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => await InvokeInDbAsync(db => db.DeleteRangeAsync<Entity>(entities));

        public virtual async Task<int> DeleteByKeyAsync<Entity>(object keyValue) => await InvokeInDbAsync(db => db.DeleteByKeyAsync<Entity>(keyValue));
        public virtual async Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => await InvokeInDbAsync(db => db.DeleteByKeysAsync<Entity, Key>(keys));
        #endregion

    }
}
