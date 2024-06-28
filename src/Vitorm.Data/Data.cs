using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Vit.Core.Module.Log;
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
            var configs = Appsettings.json.GetByPath<List<Dictionary<string, object>>>("Vitorm.Data");
            providerCache = configs?.Select(GetDataProvider).NotNull().ToList() ?? new();
        }


        #region LoadDataProvider
        public static IDataProvider DataProvider(Type entityType)
        {
            return providerMap.GetOrAdd(entityType, GetDataProviderFromConfig);

            IDataProvider GetDataProviderFromConfig(Type entityType)
            {
                var ns = entityType.Namespace;
                return providerCache.FirstOrDefault(cache => ns.StartsWith(cache.@namespace))?.dataProvider
                    ?? throw new NotImplementedException("can not find config for type: " + entityType.FullName);
            }
        }
        public static IDataProvider DataProvider<Entity>() => DataProvider(typeof(Entity));


        class DataProviderCache
        {
            public IDataProvider dataProvider;
            public string @namespace;
        }


        static readonly ConcurrentDictionary<Type, IDataProvider> providerMap = new ConcurrentDictionary<Type, IDataProvider>();

        static List<DataProviderCache> providerCache;


        static DataProviderCache GetDataProvider(Dictionary<string, object> config)
        {
            config.TryGetValue("provider", out var provider);
            config.TryGetValue("assemblyName", out var assemblyName);
            config.TryGetValue("assemblyFile", out var assemblyFile);

            Type type = ObjectLoader.GetType(className: config["provider"] as string, assemblyName: assemblyName as string, assemblyFile: assemblyFile as string);

            var dataProvider = type?.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { }) as IDataProvider;
            if (dataProvider == null) return null;

            dataProvider.Init(config);

            return new DataProviderCache
            {
                dataProvider = dataProvider,
                @namespace = config["namespace"] as string,
            };
        }


        #endregion




        #region CRUD

        // #0 Schema :  Create
        public static void Create<Entity>() => DataProvider<Entity>().Create<Entity>();


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
