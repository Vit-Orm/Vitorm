using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Vit.Linq.ExpressionNodes;
using Vit.Linq.ExpressionNodes.ExpressionConvertor.MethodCalls;

using Vitorm.Entity;

namespace Vitorm
{
    public partial class DbContext : IDbContext, IDisposable
    {
        public DbContext(Func<IDbContext, IEntityDescriptor, IDbSet> dbSetCreator = null)
        {
            this.dbSetCreator = dbSetCreator ?? Vitorm.DefaultDbSetConstructor.CreateDbSet;
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
        protected virtual Func<IDbContext, IEntityDescriptor, IDbSet> dbSetCreator { set; get; }

        protected Dictionary<Type, IDbSet> dbSetMap = null;

        public virtual IDbSet DbSet(Type entityType)
        {
            if (dbSetMap?.TryGetValue(entityType, out var dbSet) == true) return dbSet;

            var entityDescriptor = GetEntityDescriptor(entityType);

            dbSet = dbSetCreator(this, entityDescriptor);
            if (dbSet == null) return null;

            dbSetMap ??= new();
            dbSetMap[entityType] = dbSet;
            return dbSet;
        }
        public virtual IDbSet CreateDbSet(IEntityDescriptor entityDescriptor) => dbSetCreator(this, entityDescriptor);
        public virtual IDbSet<Entity> DbSet<Entity>()
        {
            return DbSet(typeof(Entity)) as IDbSet<Entity>;
        }
        #endregion


        #region EntityLoader

        public IEntityLoader entityLoader = EntityLoaders.Instance;
        public virtual IEntityDescriptor GetEntityDescriptor(Type entityType, bool tryFromCache = true)
        {
            if (tryFromCache && dbSetMap?.TryGetValue(entityType, out var dbSet) == true) return dbSet.entityDescriptor;
            return entityLoader.LoadDescriptor(entityType).entityDescriptor;
        }
        public virtual IEntityDescriptor GetEntityDescriptorFromCache(Type entityType)
        {
            if (dbSetMap?.TryGetValue(entityType, out var dbSet) == true) return dbSet.entityDescriptor;
            return default;
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
        public virtual IDbSet<Entity> ChangeTable<Entity>(string tableName)
            => ChangeTable(typeof(Entity), tableName) as IDbSet<Entity>;


        public virtual IDbSet ChangeTableBack(Type entityType)
        {
            var dbSet = DbSet(entityType);
            dbSet?.ChangeTableBack();
            return dbSet;
        }
        public virtual IDbSet<Entity> ChangeTableBack<Entity>()
            => ChangeTableBack(typeof(Entity)) as IDbSet<Entity>;

        #endregion


        public virtual void Dispose()
        {
        }




        #region Sync Method
        // #0 Schema :  Create Drop
        public virtual void TryCreateTable<Entity>() => DbSet<Entity>().TryCreateTable();
        public virtual void TryDropTable<Entity>() => DbSet<Entity>().TryDropTable();
        public virtual void Truncate<Entity>() => DbSet<Entity>().Truncate();


        // #1 Create :  Add AddRange
        public virtual Entity Add<Entity>(Entity entity) => DbSet<Entity>().Add(entity);
        public virtual void AddRange<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().AddRange(entities);


        // #2 Retrieve : Get
        public virtual Entity Get<Entity>(object keyValue) => DbSet<Entity>().Get(keyValue);
        public virtual IQueryable<Entity> Query<Entity>() => DbSet<Entity>().Query();


        // #3 Update: Update UpdateRange
        public virtual int Update<Entity>(Entity entity) => DbSet<Entity>().Update(entity);
        public virtual int UpdateRange<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().UpdateRange(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual int Delete<Entity>(Entity entity) => DbSet<Entity>().Delete(entity);
        public virtual int DeleteRange<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().DeleteRange(entities);

        public virtual int DeleteByKey<Entity>(object keyValue) => DbSet<Entity>().DeleteByKey(keyValue);
        public virtual int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => DbSet<Entity>().DeleteByKeys<Key>(keys);

        #endregion



        #region Async Method

        // #0 Schema :  Create Drop Truncate
        public virtual Task TryCreateTableAsync<Entity>() => DbSet<Entity>().TryCreateTableAsync();
        public virtual Task TryDropTableAsync<Entity>() => DbSet<Entity>().TryDropTableAsync();
        public virtual Task TruncateAsync<Entity>() => DbSet<Entity>().TruncateAsync();


        // #1 Create :  Add AddRange
        public virtual Task<Entity> AddAsync<Entity>(Entity entity) => DbSet<Entity>().AddAsync(entity);
        public virtual Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().AddRangeAsync(entities);



        // #2 Retrieve : Get Query
        public virtual Task<Entity> GetAsync<Entity>(object keyValue) => DbSet<Entity>().GetAsync(keyValue);


        // #3 Update: Update UpdateRange
        public virtual Task<int> UpdateAsync<Entity>(Entity entity) => DbSet<Entity>().UpdateAsync(entity);
        public virtual Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().UpdateRangeAsync(entities);



        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual Task<int> DeleteAsync<Entity>(Entity entity) => DbSet<Entity>().DeleteAsync(entity);
        public virtual Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().DeleteRangeAsync(entities);

        public virtual Task<int> DeleteByKeyAsync<Entity>(object keyValue) => DbSet<Entity>().DeleteByKeyAsync(keyValue);
        public virtual Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => DbSet<Entity>().DeleteByKeysAsync<Key>(keys);
        #endregion



    }
}
