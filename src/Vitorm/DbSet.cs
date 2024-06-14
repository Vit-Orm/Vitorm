using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Vitorm.Entity;

namespace Vitorm
{
    public interface IDbSet
    {
        IEntityDescriptor entityDescriptor { get; }
    }

    public class DbSetConstructor
    {
        public static IDbSet CreateDbSet(DbContext dbContext, Type entityType, IEntityDescriptor entityDescriptor)
        {
            return _CreateDbSet.MakeGenericMethod(entityType)
                     .Invoke(null, new object[] { dbContext, entityDescriptor }) as IDbSet;
        }

        static MethodInfo _CreateDbSet = new Func<DbContext, IEntityDescriptor, IDbSet>(CreateDbSet<object>)
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

        public virtual void Create() => dbContext.Create<Entity>();



        public virtual Entity Add(Entity entity) => dbContext.Add(entity);
        public virtual void AddRange(IEnumerable<Entity> entitys) => dbContext.AddRange(entitys);




        public virtual Entity Get(object keyValue) => dbContext.Get<Entity>(keyValue);
        public virtual IQueryable<Entity> Query() => dbContext.Query<Entity>();



        public virtual int Update(Entity entity) => dbContext.Update<Entity>(entity);
        public virtual int UpdateRange(IEnumerable<Entity> entitys) => dbContext.UpdateRange<Entity>(entitys);



        public virtual int Delete(Entity entity) => dbContext.Delete<Entity>(entity);
        public virtual int DeleteRange(IEnumerable<Entity> entitys) => dbContext.DeleteRange<Entity>(entitys);
        public virtual int DeleteByKey(object keyValue) => dbContext.DeleteByKey<Entity>(keyValue);
        public virtual int DeleteByKeys<Key>(IEnumerable<Key> keys) => dbContext.DeleteByKeys<Entity, Key>(keys);



    }
}
