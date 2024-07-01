using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree;

using Vitorm.Entity;
using Vitorm.Entity.Loader;

namespace Vitorm
{
    public class DbContext : IDbContext, IDisposable
    {
        public DbContext()
        {
            dbSetCreator = DefaultDbSetCreator;
        }


        #region DbSet

        public IDbSet DefaultDbSetCreator(Type entityType)
        {
            var entityDescriptor = GetEntityDescriptor(entityType);
            return DbSetConstructor.CreateDbSet(this, entityType, entityDescriptor);
        }

        protected virtual Func<Type, IDbSet> dbSetCreator { set; get; }

        Dictionary<Type, IDbSet> dbSetMap = null;

        public virtual IDbSet DbSet(Type entityType)
        {
            if (dbSetMap?.TryGetValue(entityType, out var dbSet) == true) return dbSet;

            dbSet = dbSetCreator(entityType);
            if (dbSet == null) return null;

            dbSetMap ??= new();
            dbSetMap[entityType] = dbSet;
            return dbSet;
        }
        public virtual DbSet<Entity> DbSet<Entity>()
        {
            return DbSet(typeof(Entity)) as DbSet<Entity>;
        }
        #endregion

        public static DefaultEntityLoader defaultEntityLoader = new();

        public IEntityLoader entityLoader = defaultEntityLoader;

        public virtual IEntityDescriptor GetEntityDescriptor(Type entityType) => entityLoader.LoadDescriptor(entityType);


        public virtual ExpressionConvertService convertService => Environment.convertService;



        // #0 Schema :  Create Drop
        public virtual void Create<Entity>() => throw new NotImplementedException();
        public virtual void Drop<Entity>() => throw new NotImplementedException();


        // #1 Create :  Add AddRange
        public virtual Entity Add<Entity>(Entity entity) => throw new NotImplementedException();
        public virtual void AddRange<Entity>(IEnumerable<Entity> entities) => throw new NotImplementedException();

        // #2 Retrieve : Get Query
        public virtual Entity Get<Entity>(object keyValue) => throw new NotImplementedException();
        public virtual IQueryable<Entity> Query<Entity>() => throw new NotImplementedException();


        // #3 Update: Update UpdateRange
        public virtual int Update<Entity>(Entity entity) => throw new NotImplementedException();
        public virtual int UpdateRange<Entity>(IEnumerable<Entity> entities) => throw new NotImplementedException();


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual int Delete<Entity>(Entity entity) => throw new NotImplementedException();
        public virtual int DeleteRange<Entity>(IEnumerable<Entity> entities) => throw new NotImplementedException();


        public virtual int DeleteByKey<Entity>(object keyValue) => throw new NotImplementedException();
        public virtual int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => throw new NotImplementedException();




        public virtual void Dispose()
        {
        }
    }
}
