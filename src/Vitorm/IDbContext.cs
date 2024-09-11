using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vitorm
{
    public partial interface IDbContext
    {
        #region Sync Method

        // #0 Schema :  Create Drop
        void TryCreateTable<Entity>();
        void TryDropTable<Entity>();
        void Truncate<Entity>();


        // #1 Create :  Add AddRange
        Entity Add<Entity>(Entity entity);
        void AddRange<Entity>(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        Entity Get<Entity>(object keyValue);
        IQueryable<Entity> Query<Entity>();


        // #3 Update: Update UpdateRange
        int Update<Entity>(Entity entity);
        int UpdateRange<Entity>(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        int Delete<Entity>(Entity entity);
        int DeleteRange<Entity>(IEnumerable<Entity> entities);

        int DeleteByKey<Entity>(object keyValue);
        int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys);

        #endregion



        #region Async Method

        // #0 Schema :  Create Drop
        Task TryCreateTableAsync<Entity>();
        Task TryDropTableAsync<Entity>();
        Task TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        Task<Entity> AddAsync<Entity>(Entity entity);
        Task AddRangeAsync<Entity>(IEnumerable<Entity> entities);

        // #2 Retrieve : Get Query
        Task<Entity> GetAsync<Entity>(object keyValue);
        //IQueryable<Entity> Query<Entity>();


        // #3 Update: Update UpdateRange
        Task<int> UpdateAsync<Entity>(Entity entity);
        Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        Task<int> DeleteAsync<Entity>(Entity entity);
        Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities);

        Task<int> DeleteByKeyAsync<Entity>(object keyValue);
        Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys);

        #endregion
    }
}
