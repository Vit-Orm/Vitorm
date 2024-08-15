using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Vit.Core.Util.ConfigurationManager;
using Vit.Core.Util.Reflection;
using Vit.Linq;

using Vitorm.DataProvider;

namespace Vitorm
{
    public partial class Data
    {

        static Data()
        {
            var dataSourceConfigs = Appsettings.json.GetByPath<List<Dictionary<string, object>>>("Vitorm.Data");
            var dataProviders = dataSourceConfigs?.Select(CreateDataProvider).NotNull().ToList();

            if (dataProviders?.Any() == true) providerCache.AddRange(dataProviders);
        }

        public static bool AddDataSource(Dictionary<string, object> dataSourceConfig)
        {
            var provider = CreateDataProvider(dataSourceConfig);
            if (provider == null) return false;

            providerCache.Insert(0, provider);
            providerMap.Clear();
            return true;
        }
        public static void ClearDataSource(Predicate<DataProviderCache> predicate = null)
        {
            if (predicate != null)
                providerCache.RemoveAll(predicate);
            else
                providerCache.Clear();
            providerMap.Clear();
        }


        #region LoadDataProvider

        public static IDataProvider DataProvider<Entity>() => DataProvider(typeof(Entity));
        public static IDataProvider DataProvider(Type entityType)
        {
            return providerMap.GetOrAdd(entityType, GetDataProviderFromConfig);

            static IDataProvider GetDataProviderFromConfig(Type entityType)
            {
                var FullName = entityType.FullName;
                return providerCache.FirstOrDefault(cache => cache.Match(FullName))?.dataProvider
                    ?? throw new NotImplementedException("can not find config for type: " + entityType.FullName);
            }
        }
        public static IDataProvider DataProvider(string @namespace)
        {
            return providerCache.FirstOrDefault(cache => cache.@namespace == @namespace)?.dataProvider;
        }


        static readonly ConcurrentDictionary<Type, IDataProvider> providerMap = new();

        static readonly List<DataProviderCache> providerCache = new();


        static DataProviderCache CreateDataProvider(Dictionary<string, object> dataSourceConfig)
        {
            /*
            "provider": "Vitorm.Sqlite.DataProvider",
            "assemblyName": "Vitorm.Sqlite",
            "assemblyFile": "Vitorm.Sqlite.dll",
             */

            object temp;
            string provider = dataSourceConfig.TryGetValue("provider", out temp) ? temp as string : null;
            string assemblyName = dataSourceConfig.TryGetValue("assemblyName", out temp) ? temp as string : null;
            string assemblyFile = dataSourceConfig.TryGetValue("assemblyFile", out temp) ? temp as string : null;

            Type providerType;
            IDataProvider dataProvider;

            // #1 load
            providerType = ObjectLoader.GetType(className: provider, assemblyName: assemblyName, assemblyFile: assemblyFile);
            dataProvider = providerType?.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { }) as IDataProvider;

            // #2 try load by database type (Sqlite/MySql/SqlServer)
            if (dataProvider == null)
            {
                providerType = ObjectLoader.GetType(className: $"Vitorm.{provider}.DataProvider", assemblyName: $"Vitorm.{provider}", assemblyFile: $"Vitorm.{provider}.dll");
                dataProvider = providerType?.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { }) as IDataProvider;
            }


            if (dataProvider == null) return null;

            dataProvider.Init(dataSourceConfig);
            return new DataProviderCache(dataProvider, dataSourceConfig);
        }


        #endregion




        #region CRUD

        // #0 Schema :  TryCreateTable TryDropTable
        public static void TryCreateTable<Entity>() => DataProvider<Entity>().TryCreateTable<Entity>();
        public static void TryDropTable<Entity>() => DataProvider<Entity>().TryDropTable<Entity>();
        public static void Truncate<Entity>() => DataProvider<Entity>().Truncate<Entity>();


        // #1 Create :  Add AddRange
        public static Entity Add<Entity>(Entity entity) => DataProvider<Entity>().Add<Entity>(entity);
        public static void AddRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().AddRange<Entity>(entities);

        // #2 Retrieve : Get Query
        public static Entity Get<Entity>(object keyValue) => DataProvider<Entity>().Get<Entity>(keyValue);
        public static IQueryable<Entity> Query<Entity>() => DataProvider<Entity>().Query<Entity>();


        // #3 Update: Update UpdateRange
        public static int Update<Entity>(Entity entity) => DataProvider<Entity>().Update<Entity>(entity);
        public static int UpdateRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().UpdateRange<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public static int Delete<Entity>(Entity entity) => DataProvider<Entity>().Delete<Entity>(entity);
        public static int DeleteRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().DeleteRange<Entity>(entities);

        public static int DeleteByKey<Entity>(object keyValue) => DataProvider<Entity>().DeleteByKey<Entity>(keyValue);
        public static int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => DataProvider<Entity>().DeleteByKeys<Entity, Key>(keys);


        #endregion

    }
}
