using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Vit.Core.Util.ConfigurationManager;
using Vit.Core.Util.Reflection;
using Vit.Linq;

using Vitorm.DataProvider;
using Vitorm.Entity;

namespace Vitorm
{
    public partial class Data
    {
        public static DataSource dataSource = new DataSource();

        static Data()
        {
            Init(Appsettings.json);
        }

        /// <summary>
        /// Data.Init("appsettings.Development.json")
        /// </summary>
        /// <param name="appsettingsFileName"></param>
        public static void Init(string appsettingsFileName)
        {
            Init(new JsonFile(appsettingsFileName));
        }

        public static void Init(JsonFile json)
        {
            // #1 load dataProviders
            dataSource.AddDataProviders(json);


            // #2 load DefaultEntityLoaders
            var entityLoaderConfigs = json.GetByPath<List<Dictionary<string, object>>>("Vitorm.EntityLoader");
            LoadDefaultEntityLoaders(entityLoaderConfigs);
        }

        public static void LoadDefaultEntityLoaders(IEnumerable<Dictionary<string, object>> entityLoaderConfigs)
        {
            entityLoaderConfigs?.ForEach(config =>
            {
                object temp;
                string className = config.TryGetValue("className", out temp) ? temp as string : null;
                string assemblyFile = config.TryGetValue("assemblyFile", out temp) ? temp as string : null;
                string assemblyName = config.TryGetValue("assemblyName", out temp) ? temp as string : null;

                int index = config.TryGetValue("index", out temp) && temp is int i ? i : 0;

                var entityLoader = ObjectLoader.CreateInstance(className, assemblyFile: assemblyFile, assemblyName: assemblyName) as IEntityLoader;
                if (entityLoader == null) return;

                EntityLoaders.Instance.loaders.Insert(index, entityLoader);
            });
        }

        #region DataProvider

        public static bool AddDataProvider(Dictionary<string, object> dataProviderConfig) => dataSource.AddDataProvider(dataProviderConfig);
        public static int AddDataProviders(IEnumerable<Dictionary<string, object>> dataProviderConfigs) => dataSource.AddDataProviders(dataProviderConfigs);

        public static void ClearDataProviders(Predicate<DataProviderCache> predicate = null) => dataSource.ClearDataProviders(predicate);




        public static IDataProvider DataProvider<Entity>() => dataSource.DataProvider<Entity>();
        public static IDataProvider DataProvider(Type entityType) => dataSource.DataProvider(entityType);

        /// <summary>
        /// dataProviderName:  dataProviderName or dataProviderNamespace
        /// </summary>
        /// <param name="nameOrNamespace">dataProviderName or dataProviderNamespace</param>
        /// <returns></returns>
        public static IDataProvider DataProvider(string nameOrNamespace) => dataSource.DataProvider(nameOrNamespace);

        #endregion




        #region CRUD Sync

        // #0 Schema :  TryCreateTable TryDropTable
        public static void TryCreateTable<Entity>() => dataSource.TryCreateTable<Entity>();
        public static void TryDropTable<Entity>() => dataSource.TryDropTable<Entity>();
        public static void Truncate<Entity>() => dataSource.Truncate<Entity>();


        // #1 Create :  Add AddRange
        public static Entity Add<Entity>(Entity entity) => dataSource.Add<Entity>(entity);
        public static void AddRange<Entity>(IEnumerable<Entity> entities) => dataSource.AddRange<Entity>(entities);

        // #2 Retrieve : Get Query
        public static Entity Get<Entity>(object keyValue) => dataSource.Get<Entity>(keyValue);
        public static IQueryable<Entity> Query<Entity>() => dataSource.Query<Entity>();


        // #3 Update: Update UpdateRange
        public static int Update<Entity>(Entity entity) => dataSource.Update<Entity>(entity);
        public static int UpdateRange<Entity>(IEnumerable<Entity> entities) => dataSource.UpdateRange<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public static int Delete<Entity>(Entity entity) => dataSource.Delete<Entity>(entity);
        public static int DeleteRange<Entity>(IEnumerable<Entity> entities) => dataSource.DeleteRange<Entity>(entities);

        public static int DeleteByKey<Entity>(object keyValue) => dataSource.DeleteByKey<Entity>(keyValue);
        public static int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => dataSource.DeleteByKeys<Entity, Key>(keys);

        #endregion



        #region CRUD Async

        // #0 Schema :  TryCreateTable TryDropTable
        public static Task TryCreateTableAsync<Entity>() => dataSource.TryCreateTableAsync<Entity>();
        public static Task TryDropTableAsync<Entity>() => dataSource.TryDropTableAsync<Entity>();
        public static Task TruncateAsync<Entity>() => dataSource.TruncateAsync<Entity>();


        // #1 Create :  Add AddRange
        public static Task<Entity> AddAsync<Entity>(Entity entity) => dataSource.AddAsync<Entity>(entity);
        public static Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => dataSource.AddRangeAsync<Entity>(entities);

        // #2 Retrieve : Get Query
        public static Task<Entity> GetAsync<Entity>(object keyValue) => dataSource.GetAsync<Entity>(keyValue);


        // #3 Update: Update UpdateRange
        public static Task<int> UpdateAsync<Entity>(Entity entity) => dataSource.UpdateAsync<Entity>(entity);
        public static Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => dataSource.UpdateRangeAsync<Entity>(entities);


        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public static Task<int> DeleteAsync<Entity>(Entity entity) => dataSource.DeleteAsync<Entity>(entity);
        public static Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => dataSource.DeleteRangeAsync<Entity>(entities);

        public static Task<int> DeleteByKeyAsync<Entity>(object keyValue) => dataSource.DeleteByKeyAsync<Entity>(keyValue);
        public static Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => dataSource.DeleteByKeysAsync<Entity, Key>(keys);

        #endregion

    }
}
