using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Vitorm.Entity;

namespace Vitorm
{
    public class DefaultDbSetConstructor
    {
        public static IDbSet CreateDbSet(IDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return _CreateDbSet.MakeGenericMethod(entityDescriptor.entityType)
                     .Invoke(null, new object[] { dbContext, entityDescriptor }) as IDbSet;
        }

        static readonly MethodInfo _CreateDbSet = new Func<IDbContext, IEntityDescriptor, IDbSet>(CreateDbSet<object>)
                   .Method.GetGenericMethodDefinition();
        public static IDbSet<Entity> CreateDbSet<Entity>(IDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return new DbSet<Entity>(dbContext, entityDescriptor);
        }

    }


    public partial class DbSet<Entity> : IDbSet<Entity>
    {
        public virtual IDbContext dbContext { get; protected set; }

        protected IEntityDescriptor _entityDescriptor;
        public virtual IEntityDescriptor entityDescriptor => _entityDescriptor;


        public DbSet(IDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            this.dbContext = dbContext;
            this._entityDescriptor = entityDescriptor;
        }

        // #0 Schema :  ChangeTable
        public virtual IEntityDescriptor ChangeTable(string tableName) => _entityDescriptor = _entityDescriptor.WithTable(tableName);
        public virtual IEntityDescriptor ChangeTableBack() => _entityDescriptor = _entityDescriptor.GetOriginEntityDescriptor();



        #region Sync Method

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


        #endregion





        #region Async


        // #0 Schema :  Create Drop Truncate
        public virtual Task TryCreateTableAsync() => dbContext.TryCreateTableAsync<Entity>();
        public virtual Task TryDropTableAsync() => dbContext.TryDropTableAsync<Entity>();
        public virtual Task TruncateAsync() => dbContext.TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        public virtual Task<Entity> AddAsync(Entity entity) => dbContext.AddAsync<Entity>(entity);
        public virtual Task AddRangeAsync(IEnumerable<Entity> entities) => dbContext.AddRangeAsync<Entity>(entities);


        // #2 Retrieve : Get Query
        public virtual Task<Entity> GetAsync(object keyValue) => dbContext.GetAsync<Entity>(keyValue);


        // #3 Update: Update UpdateRange
        public virtual Task<int> UpdateAsync(Entity entity) => dbContext.UpdateAsync<Entity>(entity);
        public virtual Task<int> UpdateRangeAsync(IEnumerable<Entity> entities) => dbContext.UpdateRangeAsync<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual Task<int> DeleteAsync(Entity entity) => dbContext.DeleteAsync<Entity>(entity);
        public virtual Task<int> DeleteRangeAsync(IEnumerable<Entity> entities) => dbContext.DeleteRangeAsync<Entity>(entities);
        public virtual Task<int> DeleteByKeyAsync(object keyValue) => dbContext.DeleteByKeyAsync<Entity>(keyValue);
        public virtual Task<int> DeleteByKeysAsync<Key>(IEnumerable<Key> keys) => dbContext.DeleteByKeysAsync<Entity, Key>(keys);

        #endregion

    }
}
