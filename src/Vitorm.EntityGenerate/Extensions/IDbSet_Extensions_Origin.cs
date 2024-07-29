using System.Collections.Generic;
using System.Linq;
namespace Vitorm
{
    public static partial class IDbSet_Extensions
    {
        // #0 Schema :  Create Drop
        private static void TryCreateTable<Entity>(this IDbSet data) => (data as DbSet<Entity>).TryCreateTable();
        private static void TryDropTable<Entity>(this IDbSet data) => (data as DbSet<Entity>).TryDropTable();


        // #1 Create :  Add AddRange
        private static Entity Add<Entity>(this IDbSet data, Entity entity) => (data as DbSet<Entity>).Add(entity);
        private static void AddRange<Entity>(this IDbSet data, IEnumerable<Entity> entities) => (data as DbSet<Entity>).AddRange(entities);




        // #2 Retrieve : Get Query
        private static Entity Get<Entity>(this IDbSet data, object keyValue) => (data as DbSet<Entity>).Get(keyValue);
        private static IQueryable<Entity> Query<Entity>(this IDbSet data) => (data as DbSet<Entity>).Query();


        // #3 Update: Update UpdateRange
        private static int Update<Entity>(this IDbSet data, Entity entity) => (data as DbSet<Entity>).Update(entity);
        private static int UpdateRange<Entity>(this IDbSet data, IEnumerable<Entity> entities) => (data as DbSet<Entity>).UpdateRange(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        private static int Delete<Entity>(this IDbSet data, Entity entity) => (data as DbSet<Entity>).Delete(entity);
        private static int DeleteRange<Entity>(this IDbSet data, IEnumerable<Entity> entities) => (data as DbSet<Entity>).DeleteRange(entities);

        private static int DeleteByKey<Entity>(this IDbSet data, object keyValue) => (data as DbSet<Entity>).DeleteByKey(keyValue);
        private static int DeleteByKeys<Entity, Key>(this IDbSet data, IEnumerable<Key> keys) => (data as DbSet<Entity>).DeleteByKeys(keys);



    }
}
