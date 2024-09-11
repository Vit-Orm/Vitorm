using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Vitorm.Entity;

namespace Vitorm
{
    public partial interface IDbSet
    {
        IEntityDescriptor entityDescriptor { get; }
        IDbContext dbContext { get; }


        #region Sync Method

        // #0 Schema :  ChangeTable
        IEntityDescriptor ChangeTable(string tableName);
        IEntityDescriptor ChangeTableBack();

        // #0 Schema :  Create Drop Truncate
        void TryCreateTable();
        void TryDropTable();
        void Truncate();
        #endregion


        #region Async Method

        // #0 Schema :  Create Drop Truncate
        Task TryCreateTableAsync();
        Task TryDropTableAsync();
        Task TruncateAsync();
        #endregion
    }

    public partial interface IDbSet<Entity> : IDbSet
    {
        #region Sync Method

        // #1 Create :  Add AddRange
        Entity Add(Entity entity);
        void AddRange(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        Entity Get(object keyValue);
        IQueryable<Entity> Query();


        // #3 Update: Update UpdateRange
        int Update(Entity entity);
        int UpdateRange(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        int Delete(Entity entity);
        int DeleteRange(IEnumerable<Entity> entities);
        int DeleteByKey(object keyValue);
        int DeleteByKeys<Key>(IEnumerable<Key> keys);
        #endregion


        #region Async Method

        // #1 Create :  Add AddRange
        Task<Entity> AddAsync(Entity entity);
        Task AddRangeAsync(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        Task<Entity> GetAsync(object keyValue);
        //IQueryable<Entity> Query();


        // #3 Update: Update UpdateRange
        Task<int> UpdateAsync(Entity entity);
        Task<int> UpdateRangeAsync(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        Task<int> DeleteAsync(Entity entity);
        Task<int> DeleteRangeAsync(IEnumerable<Entity> entities);
        Task<int> DeleteByKeyAsync(object keyValue);
        Task<int> DeleteByKeysAsync<Key>(IEnumerable<Key> keys);

        #endregion

    }
}
