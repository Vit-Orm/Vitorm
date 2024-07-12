using System;
using System.Collections.Generic;
using System.Linq;

using Vitorm.Sql;

namespace Vitorm.DataProvider
{
    public abstract class SqlDataProvider : IDataProvider
    {
        DbContext IDataProvider.CreateDbContext() => this.CreateDbContext();

        public abstract SqlDbContext CreateDbContext();

        public abstract void Init(Dictionary<string, object> config);


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

    }
}
