using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree;
using Vit.Linq.ExpressionTree.ExpressionConvertor.MethodCalls;

using Vitorm.Entity;
using Vitorm.Entity.Loader;

namespace Vitorm
{
    public abstract partial class DbContext : IDbContext, IDisposable
    {
        public DbContext()
        {
            dbSetCreator = DefaultDbSetCreator;
        }


        #region ExpressionConvertService
        public static ExpressionConvertService CreateDefaultExpressionConvertService()
        {
            var convertService = new ExpressionConvertService();
            convertService.RegisterMethodConvertor(new MethodConvertor_ForType(typeof(DbFunction)));
            convertService.RegisterMethodConvertor(new MethodConvertor_ForType(typeof(Orm_Extensions)));
            return convertService;
        }
        public static ExpressionConvertService defaultExpressionConvertService = CreateDefaultExpressionConvertService();

        public virtual ExpressionConvertService convertService => defaultExpressionConvertService;
        #endregion


        #region DbSet
        protected IDbSet DefaultDbSetCreator(IEntityDescriptor entityDescriptor)
        {
            return DbSetConstructor.CreateDbSet(this, entityDescriptor);
        }

        protected virtual Func<IEntityDescriptor, IDbSet> dbSetCreator { set; get; }

        protected Dictionary<Type, IDbSet> dbSetMap = null;

        public virtual IDbSet DbSet(Type entityType)
        {
            if (dbSetMap?.TryGetValue(entityType, out var dbSet) == true) return dbSet;

            var entityDescriptor = GetEntityDescriptor(entityType);

            dbSet = dbSetCreator(entityDescriptor);
            if (dbSet == null) return null;

            dbSetMap ??= new();
            dbSetMap[entityType] = dbSet;
            return dbSet;
        }
        public virtual IDbSet CreateDbSet(IEntityDescriptor entityDescriptor) => dbSetCreator(entityDescriptor);
        public virtual IDbSet<Entity> DbSet<Entity>()
        {
            return DbSet(typeof(Entity)) as IDbSet<Entity>;
        }
        #endregion


        #region EntityLoader

        public static DefaultEntityLoader defaultEntityLoader = new();

        public IEntityLoader entityLoader = defaultEntityLoader;
        public virtual IEntityDescriptor GetEntityDescriptor(Type entityType, bool tryFromCache = true)
        {
            if (tryFromCache && dbSetMap?.TryGetValue(entityType, out var dbSet) == true) return dbSet.entityDescriptor;
            return entityLoader.LoadDescriptor(entityType);
        }
        public virtual IEntityDescriptor GetEntityDescriptor<Entity>(bool tryFromCache = true)
            => GetEntityDescriptor(typeof(Entity), tryFromCache);
        #endregion



        #region ChangeTable ChangeTableBack

        public virtual IDbSet ChangeTable(Type entityType, string tableName)
        {
            var dbSet = DbSet(entityType);
            dbSet?.ChangeTable(tableName);
            return dbSet;
        }
        public virtual DbSet<Entity> ChangeTable<Entity>(string tableName)
            => ChangeTable(typeof(Entity), tableName) as DbSet<Entity>;


        public virtual IDbSet ChangeTableBack(Type entityType)
        {
            var dbSet = DbSet(entityType);
            dbSet?.ChangeTableBack();
            return dbSet;
        }
        public virtual DbSet<Entity> ChangeTableBack<Entity>()
            => ChangeTableBack(typeof(Entity)) as DbSet<Entity>;

        #endregion





        public virtual void Dispose()
        {
        }



        // #0 Schema :  Create Drop
        public abstract void TryCreateTable<Entity>();
        public abstract void TryDropTable<Entity>();
        public abstract void Truncate<Entity>();


        // #1 Create :  Add AddRange
        public abstract Entity Add<Entity>(Entity entity);
        public abstract void AddRange<Entity>(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        public abstract Entity Get<Entity>(object keyValue);
        public abstract IQueryable<Entity> Query<Entity>();


        // #3 Update: Update UpdateRange
        public abstract int Update<Entity>(Entity entity);
        public abstract int UpdateRange<Entity>(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public abstract int Delete<Entity>(Entity entity);
        public abstract int DeleteRange<Entity>(IEnumerable<Entity> entities);


        public abstract int DeleteByKey<Entity>(object keyValue);
        public abstract int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys);





    }
}
