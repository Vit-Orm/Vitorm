using System.Collections.Generic;
using System.Linq;
namespace Vitorm
{
    public static partial class IDbSet_Extensions
    {
        // #0 Schema :  Create Drop
        private static void TryCreateTable<Entity>(this IDbSet data) => ((IDbSet<Entity>)data).TryCreateTable();
        private static void TryDropTable<Entity>(this IDbSet data) => ((IDbSet<Entity>)data).TryDropTable();


        // #1 Create :  Add AddRange
        private static Entity Add<Entity>(this IDbSet data, Entity entity) => ((IDbSet<Entity>)data).Add(entity);
        private static void AddRange<Entity>(this IDbSet data, IEnumerable<Entity> entities) => ((IDbSet<Entity>)data).AddRange(entities);




        // #2 Retrieve : Get Query
        private static Entity Get<Entity>(this IDbSet data, object keyValue) => ((IDbSet<Entity>)data).Get(keyValue);
        private static IQueryable<Entity> Query<Entity>(this IDbSet data) => ((IDbSet<Entity>)data).Query();


        // #3 Update: Update UpdateRange
        private static int Update<Entity>(this IDbSet data, Entity entity) => ((IDbSet<Entity>)data).Update(entity);
        private static int UpdateRange<Entity>(this IDbSet data, IEnumerable<Entity> entities) => ((IDbSet<Entity>)data).UpdateRange(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        private static int Delete<Entity>(this IDbSet data, Entity entity) => ((IDbSet<Entity>)data).Delete(entity);
        private static int DeleteRange<Entity>(this IDbSet data, IEnumerable<Entity> entities) => ((IDbSet<Entity>)data).DeleteRange(entities);

        private static int DeleteByKey<Entity>(this IDbSet data, object keyValue) => ((IDbSet<Entity>)data).DeleteByKey(keyValue);
        private static int DeleteByKeys<Entity, Key>(this IDbSet data, IEnumerable<Key> keys) => ((IDbSet<Entity>)data).DeleteByKeys(keys);



    }
}
