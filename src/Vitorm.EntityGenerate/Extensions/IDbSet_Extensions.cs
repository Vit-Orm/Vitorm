using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Vit.Core.Module.Serialization;
using Vit.Linq;

using EntityType = System.Object;
namespace Vitorm
{
    public static partial class IDbSet_Extensions
    {
        #region #0 Schema :  Create Drop
        public static void TryCreateTable(this IDbSet data)
        {
            var entityType = data.entityDescriptor.entityType;

            var methodInfo = new Action<IDbSet>(TryCreateTable<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            methodInfo.Invoke(null, new object[] { data });
        }
        public static void TryDropTable(this IDbSet data)
        {
            var entityType = data.entityDescriptor.entityType;

            var methodInfo = new Action<IDbSet>(TryDropTable<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            methodInfo.Invoke(null, new object[] { data });
        }

        #endregion

        #region #1 Create :  Add AddRange
        public static object Add(this IDbSet data, object entity)
        {
            if (entity == null) return null;

            var entityType = data.entityDescriptor.entityType;
            if (!entityType.IsInstanceOfType(entity)) entity = Json.Deserialize(Json.Serialize(entity), entityType);

            var methodInfo = new Func<IDbSet, EntityType, EntityType>(Add<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return methodInfo.Invoke(null, new object[] { data, entity });
        }
        public static void AddRange(this IDbSet data, object entities)
        {
            if (entities == null) return;
            var entityType = data.entityDescriptor.entityType;
            var entityListType = typeof(IEnumerable<>).MakeGenericType(entityType);
            if (!entityListType.IsAssignableFrom(entities.GetType())) entities = Json.Deserialize(Json.Serialize(entities), typeof(List<>).MakeGenericType(entityType));

            var methodInfo = new Action<IDbSet, IEnumerable<EntityType>>(AddRange<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            methodInfo.Invoke(null, new object[] { data, entities });
        }
        #endregion




        #region #2 Retrieve : Get Query

        public static object Get(this IDbSet data, object keyValue)
        {
            var entityType = data.entityDescriptor.entityType;
            var methodInfo = new Func<IDbSet, object, EntityType>(Get<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return methodInfo.Invoke(null, new object[] { data, keyValue });
        }
        public static IQueryable Query(this IDbSet data)
        {
            var entityType = data.entityDescriptor.entityType;

            var methodInfo = new Func<IDbSet, IQueryable<EntityType>>(Query<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return methodInfo.Invoke(null, new object[] { data }) as IQueryable;
        }
        #endregion


        #region #3 Update: Update UpdateRange
        public static int Update(this IDbSet data, object entity)
        {
            if (entity == null) return default;

            var entityType = data.entityDescriptor.entityType;
            if (!entityType.IsInstanceOfType(entity)) entity = Json.Deserialize(Json.Serialize(entity), entityType);

            var methodInfo = new Func<IDbSet, EntityType, int>(Update<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return (int)methodInfo.Invoke(null, new object[] { data, entity });
        }
        public static int UpdateRange(this IDbSet data, object entities)
        {
            if (entities == null) return default;
            var entityType = data.entityDescriptor.entityType;
            var entityListType = typeof(IEnumerable<>).MakeGenericType(entityType);
            if (!entityListType.IsAssignableFrom(entities.GetType())) entities = Json.Deserialize(Json.Serialize(entities), typeof(List<>).MakeGenericType(entityType));

            var methodInfo = new Func<IDbSet, IEnumerable<EntityType>, int>(UpdateRange<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return (int)methodInfo.Invoke(null, new object[] { data, entities });
        }
        #endregion


        #region #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public static int Delete(this IDbSet data, object entity)
        {
            if (entity == null) return default;

            var entityType = data.entityDescriptor.entityType;
            if (!entityType.IsInstanceOfType(entity)) entity = Json.Deserialize(Json.Serialize(entity), entityType);

            var methodInfo = new Func<IDbSet, EntityType, int>(Delete<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return (int)methodInfo.Invoke(null, new object[] { data, entity });
        }
        public static int DeleteRange(this IDbSet data, object entities)
        {
            if (entities == null) return default;
            var entityType = data.entityDescriptor.entityType;
            var entityListType = typeof(IEnumerable<>).MakeGenericType(entityType);
            if (!entityListType.IsAssignableFrom(entities.GetType())) entities = Json.Deserialize(Json.Serialize(entities), typeof(List<>).MakeGenericType(entityType));

            var methodInfo = new Func<IDbSet, IEnumerable<EntityType>, int>(DeleteRange<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return (int)methodInfo.Invoke(null, new object[] { data, entities });
        }

        public static int DeleteByKey(this IDbSet data, object keyValue)
        {
            if (keyValue == null) return default;

            var entityType = data.entityDescriptor.entityType;

            var methodInfo = new Func<IDbSet, object, int>(DeleteByKey<EntityType>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType);
            return (int)methodInfo.Invoke(null, new object[] { data, keyValue });
        }
        public static int DeleteByKeys(this IDbSet data, object keys)
        {
            if (keys == null) return default;

            var entityType = data.entityDescriptor.entityType;
            var keyType = LinqHelp.GetElementType(keys.GetType());

            var methodInfo = new Func<IDbSet, IEnumerable<int>, int>(DeleteByKeys<EntityType, int>)
                    .GetMethodInfo().GetGenericMethodDefinition()
                    .MakeGenericMethod(entityType, keyType);
            return (int)methodInfo.Invoke(null, new object[] { data, keys });
        }
        #endregion

    }
}
