using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Vit.Core.Util.ConfigurationManager;
using Vit.Core.Util.Reflection;
using Vit.Extensions.Linq_Extensions;

using Vitorm.DataProvider;

namespace Vitorm
{
    public partial class Data
    {

        static Data()
        {
            var dataSourceConfigs = Appsettings.json.GetByPath<List<Dictionary<string, object>>>("Vitorm.Data");
            var dataProviders = dataSourceConfigs?.Select(GetDataProvider).NotNull().ToList();

            if (dataProviders?.Any() == true) providerCache.AddRange(dataProviders);
        }

        public static bool AddDataSource(Dictionary<string, object> dataSourceConfig)
        {
            var provider = GetDataProvider(dataSourceConfig);
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
        public static IDataProvider DataProvider(Type entityType)
        {
            return providerMap.GetOrAdd(entityType, GetDataProviderFromConfig);

            IDataProvider GetDataProviderFromConfig(Type entityType)
            {
                var Namespace = entityType.Namespace;
                return providerCache.FirstOrDefault(cache => Namespace.StartsWith(cache.@namespace))?.dataProvider
                    ?? throw new NotImplementedException("can not find config for type: " + entityType.FullName);
            }
        }
        public static IDataProvider DataProvider(string Namespace)
        {
            return providerCache.FirstOrDefault(cache => Namespace.StartsWith(cache.@namespace))?.dataProvider;
        }
        public static IDataProvider DataProvider<Entity>() => DataProvider(typeof(Entity));


        public class DataProviderCache
        {
            public IDataProvider dataProvider;
            public string @namespace;
            public Dictionary<string, object> dataSourceConfig;
        }


        static readonly ConcurrentDictionary<Type, IDataProvider> providerMap = new();

        static readonly List<DataProviderCache> providerCache = new();


        static DataProviderCache GetDataProvider(Dictionary<string, object> dataSourceConfig)
        {
            dataSourceConfig.TryGetValue("provider", out var provider);
            dataSourceConfig.TryGetValue("assemblyName", out var assemblyName);
            dataSourceConfig.TryGetValue("assemblyFile", out var assemblyFile);

            Type type = ObjectLoader.GetType(className: dataSourceConfig["provider"] as string, assemblyName: assemblyName as string, assemblyFile: assemblyFile as string);

            var dataProvider = type?.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { }) as IDataProvider;
            if (dataProvider == null) return null;

            dataProvider.Init(dataSourceConfig);

            return new DataProviderCache
            {
                dataProvider = dataProvider,
                @namespace = dataSourceConfig["namespace"] as string,
                dataSourceConfig = dataSourceConfig,
            };
        }


        #endregion




        #region CRUD

        // #0 Schema :  Create
        public static void Create<Entity>() => DataProvider<Entity>().Create<Entity>();
        public static void Drop<Entity>() => DataProvider<Entity>().Drop<Entity>();


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
