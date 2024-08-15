using System.Collections.Generic;
using System.Linq;

using Vitorm.Entity;

namespace Vitorm
{
    public interface IDbSet
    {
        IEntityDescriptor entityDescriptor { get; }
        DbContext dbContext { get; }

        // #0 Schema :  ChangeTable
        IEntityDescriptor ChangeTable(string tableName);
        IEntityDescriptor ChangeTableBack();

        // #0 Schema :  Create Drop
        void TryCreateTable();
        void TryDropTable();
        void Truncate();
    }

    public interface IDbSet<Entity> : IDbSet
    {
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


    }
}
