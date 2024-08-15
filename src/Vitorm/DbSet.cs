using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Vitorm.Entity;

namespace Vitorm
{
    public class DbSetConstructor
    {
        public static IDbSet CreateDbSet(DbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return _CreateDbSet.MakeGenericMethod(entityDescriptor.entityType)
                     .Invoke(null, new object[] { dbContext, entityDescriptor }) as IDbSet;
        }

        static readonly MethodInfo _CreateDbSet = new Func<DbContext, IEntityDescriptor, IDbSet>(CreateDbSet<object>)
                   .Method.GetGenericMethodDefinition();
        public static IDbSet<Entity> CreateDbSet<Entity>(DbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return new DbSet<Entity>(dbContext, entityDescriptor);
        }

    }


    public class DbSet<Entity> : IDbSet<Entity>
    {
        public virtual DbContext dbContext { get; protected set; }

        protected IEntityDescriptor _entityDescriptor;
        public virtual IEntityDescriptor entityDescriptor => _entityDescriptor;


        public DbSet(DbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            this.dbContext = dbContext;
            this._entityDescriptor = entityDescriptor;
        }

        // #0 Schema :  ChangeTable
        public virtual IEntityDescriptor ChangeTable(string tableName) => _entityDescriptor = _entityDescriptor.WithTable(tableName);
        public virtual IEntityDescriptor ChangeTableBack() => _entityDescriptor = _entityDescriptor.GetOriginEntityDescriptor();


        // #0 Schema :  Create Drop
        public virtual void TryCreateTable() => dbContext.TryCreateTable<Entity>();
        public virtual void TryDropTable() => dbContext.TryDropTable<Entity>();
        public virtual void Truncate() => dbContext.Truncate<Entity>();


        // #1 Create :  Add AddRange
        public virtual Entity Add(Entity entity) => dbContext.Add(entity);
        public virtual void AddRange(IEnumerable<Entity> entities) => dbContext.AddRange(entities);


        // #2 Retrieve : Get Query
        public virtual Entity Get(object keyValue) => dbContext.Get<Entity>(keyValue);
        public virtual IQueryable<Entity> Query() => dbContext.Query<Entity>();


        // #3 Update: Update UpdateRange
        public virtual int Update(Entity entity) => dbContext.Update<Entity>(entity);
        public virtual int UpdateRange(IEnumerable<Entity> entities) => dbContext.UpdateRange<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual int Delete(Entity entity) => dbContext.Delete<Entity>(entity);
        public virtual int DeleteRange(IEnumerable<Entity> entities) => dbContext.DeleteRange<Entity>(entities);
        public virtual int DeleteByKey(object keyValue) => dbContext.DeleteByKey<Entity>(keyValue);
        public virtual int DeleteByKeys<Key>(IEnumerable<Key> keys) => dbContext.DeleteByKeys<Entity, Key>(keys);


    }
}
