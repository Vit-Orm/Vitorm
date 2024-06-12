using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree;

using Vitorm.Entity;

namespace Vitorm
{
    public class DbContext : IDisposable
    {
        public DbContext() { }

        public virtual ExpressionConvertService convertService => Environment.convertService;

        public Func<Type, IDbSet> dbSetCreator { set; protected get; }

        Dictionary<Type, IDbSet> dbSetMap = new();

        public virtual IDbSet DbSet(Type entityType)
        {
            if (dbSetMap.TryGetValue(entityType, out var dbSet)) return dbSet;

            dbSet = dbSetCreator(entityType);
            if (dbSet == null) return null;
            dbSetMap[entityType] = dbSet;
            return dbSet;

            //return dbSetMap.GetOrAdd(entityType, dbSetCreator);
        }
        public virtual DbSet<Entity> DbSet<Entity>()
        {
            return DbSet(typeof(Entity)) as DbSet<Entity>;
        }


        public virtual IEntityDescriptor GetEntityDescriptor(Type entityType) => DbSet(entityType)?.entityDescriptor;


        // #1 Schema :  Create
        public virtual void Create<Entity>() => throw new NotImplementedException();


        // #1 Create :  Add AddRange
        public virtual Entity Add<Entity>(Entity entity) => throw new NotImplementedException();
        public virtual void AddRange<Entity>(IEnumerable<Entity> entitys) => throw new NotImplementedException();

        // #2 Retrieve : Get Query
        public virtual Entity Get<Entity>(object keyValue) => throw new NotImplementedException();
        public virtual IQueryable<Entity> Query<Entity>() => throw new NotImplementedException();


        // #3 Update: Update UpdateRange
        public virtual int Update<Entity>(Entity entity) => throw new NotImplementedException();
        public virtual int UpdateRange<Entity>(IEnumerable<Entity> entitys) => throw new NotImplementedException();


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual int Delete<Entity>(Entity entity) => throw new NotImplementedException();
        public virtual int DeleteRange<Entity>(IEnumerable<Entity> entitys) => throw new NotImplementedException();


        public virtual int DeleteByKey<Entity>(object keyValue) => throw new NotImplementedException();
        public virtual int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => throw new NotImplementedException();




        public virtual void Dispose()
        {
        }
    }
}
