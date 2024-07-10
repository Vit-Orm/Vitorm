using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Vitorm.Entity;

namespace Vitorm
{
    public class DbSetConstructor
    {
        public static IDbSet CreateDbSet(DbContext dbContext, Type entityType, IEntityDescriptor entityDescriptor)
        {
            return _CreateDbSet.MakeGenericMethod(entityType)
                     .Invoke(null, new object[] { dbContext, entityDescriptor }) as IDbSet;
        }

        static readonly MethodInfo _CreateDbSet = new Func<DbContext, IEntityDescriptor, IDbSet>(CreateDbSet<object>)
                   .Method.GetGenericMethodDefinition();
        public static IDbSet CreateDbSet<Entity>(DbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return new DbSet<Entity>(dbContext, entityDescriptor);
        }

    }


    public class DbSet<Entity> : IDbSet
    {
        protected DbContext dbContext;

        protected IEntityDescriptor _entityDescriptor;
        public virtual IEntityDescriptor entityDescriptor => _entityDescriptor;


        public DbSet(DbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            this.dbContext = dbContext;
            this._entityDescriptor = entityDescriptor;
        }


        public virtual IEntityDescriptor ChangeTable(string tableName) => _entityDescriptor = _entityDescriptor.WithTable(tableName);
        public virtual IEntityDescriptor ChangeTableBack() => _entityDescriptor = _entityDescriptor.GetOriginEntityDescriptor();



        public virtual void TryCreateTable() => dbContext.TryCreateTable<Entity>();
        public virtual void TryDropTable() => dbContext.TryDropTable<Entity>();



        public virtual Entity Add(Entity entity) => dbContext.Add(entity);
        public virtual void AddRange(IEnumerable<Entity> entities) => dbContext.AddRange(entities);




        public virtual Entity Get(object keyValue) => dbContext.Get<Entity>(keyValue);
        public virtual IQueryable<Entity> Query() => dbContext.Query<Entity>();



        public virtual int Update(Entity entity) => dbContext.Update<Entity>(entity);
        public virtual int UpdateRange(IEnumerable<Entity> entities) => dbContext.UpdateRange<Entity>(entities);



        public virtual int Delete(Entity entity) => dbContext.Delete<Entity>(entity);
        public virtual int DeleteRange(IEnumerable<Entity> entities) => dbContext.DeleteRange<Entity>(entities);
        public virtual int DeleteByKey(object keyValue) => dbContext.DeleteByKey<Entity>(keyValue);
        public virtual int DeleteByKeys<Key>(IEnumerable<Key> keys) => dbContext.DeleteByKeys<Entity, Key>(keys);



    }
}
