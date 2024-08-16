using System.Collections.Generic;
using System.Linq;

namespace Vitorm
{
    public partial interface IDbContext
    {
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
    }
}
