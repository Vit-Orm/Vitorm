using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Vit.Linq.ExpressionTree;
using Vit.Orm.Entity;

namespace Vit.Orm
{
    public class DbContext : IDisposable
    {
        public DbContext() { }

        public virtual ExpressionConvertService convertService => ExpressionConvertService.Instance;

        public Func<Type, IDbSet> dbSetCreator { set; protected get; }

        Dictionary<Type, IDbSet> dbSetMap = new();

        public virtual IDbSet DbSet(Type entityType)
        {
            if (dbSetMap.TryGetValue(entityType, out var dbSet)) return dbSet;

            dbSet = dbSetCreator(entityType);
            dbSetMap[entityType] = dbSet;
            return dbSet;

            //return dbSetMap.GetOrAdd(entityType, dbSetCreator);
        }
        public virtual DbSet<Entity> DbSet<Entity>()
        {
            return DbSet(typeof(Entity)) as DbSet<Entity>;
        }


        public virtual IEntityDescriptor GetEntityDescriptor(Type entityType) => DbSet(entityType)?.entityDescriptor;



        public virtual void Create<Entity>() => DbSet<Entity>().Create();

        public virtual Entity Add<Entity>(Entity entity) => DbSet<Entity>().Add(entity);
        public virtual void AddRange<Entity>(IEnumerable<Entity> entitys) => DbSet<Entity>().AddRange(entitys);


        public virtual Entity Get<Entity>(object keyValue) => DbSet<Entity>().Get(keyValue);
        public virtual IQueryable<Entity> Query<Entity>() => DbSet<Entity>().Query();



        public virtual int Update<Entity>(Entity entity) => DbSet<Entity>().Update(entity);
        public virtual int UpdateRange<Entity>(IEnumerable<Entity> entitys) => DbSet<Entity>().UpdateRange(entitys);



        public virtual int Delete<Entity>(Entity entity) => DbSet<Entity>().Delete(entity);
        public virtual int DeleteRange<Entity>(IEnumerable<Entity> entitys) => DbSet<Entity>().DeleteRange(entitys);


        public virtual int DeleteByKey<Entity>(object keyValue) => DbSet<Entity>().DeleteByKey(keyValue);
        public virtual int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => DbSet<Entity>().DeleteByKeys(keys);

        public virtual IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }
        public virtual void Dispose()
        {
        }
    }
}
