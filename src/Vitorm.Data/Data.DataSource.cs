using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Vit.Core.Util.ConfigurationManager;
using Vit.Core.Util.Reflection;
using Vit.Linq;

using Vitorm.DataProvider;

namespace Vitorm
{
    public partial class Data
    {
        public partial class DataSource
        {

            /// <summary>
            /// Data.Init("appsettings.Development.json")
            /// </summary>
            /// <param name="appsettingsFileName"></param>
            public DataSource LoadDataProviders(string appsettingsFileName)
            {
                LoadDataProviders(new JsonFile(appsettingsFileName));
                return this;
            }

            public DataSource LoadDataProviders(JsonFile json, string configPath = "Vitorm.Data")
            {
                var dataProviderConfigs = json.GetByPath<List<Dictionary<string, object>>>(configPath);
                return LoadDataProviders(dataProviderConfigs);
            }
            public DataSource LoadDataProviders(IEnumerable<Dictionary<string, object>> dataProviderConfigs)
            {
                var dataProviders = dataProviderConfigs?.Select(CreateDataProvider).NotNull().ToList();

                if (dataProviders?.Any() == true) providerCache.AddRange(dataProviders);

                providerMap.Clear();

                return this;
            }

            public bool LoadDataProvider(Dictionary<string, object> dataProviderConfig)
            {
                var provider = CreateDataProvider(dataProviderConfig);
                if (provider == null) return false;

                providerCache.Insert(0, provider);
                providerMap.Clear();
                return true;
            }
            public void ClearDataProviders(Predicate<DataProviderCache> predicate = null)
            {
                if (predicate != null)
                    providerCache.RemoveAll(predicate);
                else
                    providerCache.Clear();

                providerMap.Clear();
            }


            #region LoadDataProvider

            public IDataProvider DataProvider<Entity>() => DataProvider(typeof(Entity));
            public IDataProvider DataProvider(Type entityType)
            {
                return providerMap.GetOrAdd(entityType, GetDataProviderFromConfig);

                IDataProvider GetDataProviderFromConfig(Type entityType)
                {
                    var classFullName = entityType.FullName;
                    return providerCache.FirstOrDefault(cache => cache.Match(classFullName))?.dataProvider
                        ?? throw new NotImplementedException("can not find config for type: " + classFullName);
                }
            }

            /// <summary>
            /// dataProviderName:  dataProviderName or dataProviderNamespace
            /// </summary>
            /// <param name="dataProviderName"></param>
            /// <returns></returns>
            public IDataProvider DataProvider(string dataProviderName)
            {
                return providerCache.FirstOrDefault(cache => cache.name == dataProviderName || cache.@namespace == dataProviderName)?.dataProvider;
            }


            readonly ConcurrentDictionary<Type, IDataProvider> providerMap = new();

            readonly List<DataProviderCache> providerCache = new();


            DataProviderCache CreateDataProvider(Dictionary<string, object> dataProviderConfig)
            {
                /*
                "provider": "Vitorm.Sqlite.DataProvider",
                "assemblyName": "Vitorm.Sqlite",
                "assemblyFile": "Vitorm.Sqlite.dll",
                 */

                object temp;
                string provider = dataProviderConfig.TryGetValue("provider", out temp) ? temp as string : null;
                string assemblyName = dataProviderConfig.TryGetValue("assemblyName", out temp) ? temp as string : null;
                string assemblyFile = dataProviderConfig.TryGetValue("assemblyFile", out temp) ? temp as string : null;

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

                dataProvider.Init(dataProviderConfig);
                return new DataProviderCache(dataProvider, dataProviderConfig);
            }


            #endregion




            #region CRUD Sync

            // #0 Schema :  TryCreateTable TryDropTable
            public void TryCreateTable<Entity>() => DataProvider<Entity>().TryCreateTable<Entity>();
            public void TryDropTable<Entity>() => DataProvider<Entity>().TryDropTable<Entity>();
            public void Truncate<Entity>() => DataProvider<Entity>().Truncate<Entity>();


            // #1 Create :  Add AddRange
            public Entity Add<Entity>(Entity entity) => DataProvider<Entity>().Add<Entity>(entity);
            public void AddRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().AddRange<Entity>(entities);

            // #2 Retrieve : Get Query
            public Entity Get<Entity>(object keyValue) => DataProvider<Entity>().Get<Entity>(keyValue);
            public IQueryable<Entity> Query<Entity>() => DataProvider<Entity>().Query<Entity>();


            // #3 Update: Update UpdateRange
            public int Update<Entity>(Entity entity) => DataProvider<Entity>().Update<Entity>(entity);
            public int UpdateRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().UpdateRange<Entity>(entities);


            // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            public int Delete<Entity>(Entity entity) => DataProvider<Entity>().Delete<Entity>(entity);
            public int DeleteRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().DeleteRange<Entity>(entities);

            public int DeleteByKey<Entity>(object keyValue) => DataProvider<Entity>().DeleteByKey<Entity>(keyValue);
            public int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => DataProvider<Entity>().DeleteByKeys<Entity, Key>(keys);

            #endregion



            #region CRUD Async

            // #0 Schema :  TryCreateTable TryDropTable
            public Task TryCreateTableAsync<Entity>() => DataProvider<Entity>().TryCreateTableAsync<Entity>();
            public Task TryDropTableAsync<Entity>() => DataProvider<Entity>().TryDropTableAsync<Entity>();
            public Task TruncateAsync<Entity>() => DataProvider<Entity>().TruncateAsync<Entity>();


            // #1 Create :  Add AddRange
            public Task<Entity> AddAsync<Entity>(Entity entity) => DataProvider<Entity>().AddAsync<Entity>(entity);
            public Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().AddRangeAsync<Entity>(entities);

            // #2 Retrieve : Get Query
            public Task<Entity> GetAsync<Entity>(object keyValue) => DataProvider<Entity>().GetAsync<Entity>(keyValue);


            // #3 Update: Update UpdateRange
            public Task<int> UpdateAsync<Entity>(Entity entity) => DataProvider<Entity>().UpdateAsync<Entity>(entity);
            public Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().UpdateRangeAsync<Entity>(entities);


            // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            public Task<int> DeleteAsync<Entity>(Entity entity) => DataProvider<Entity>().DeleteAsync<Entity>(entity);
            public Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().DeleteRangeAsync<Entity>(entities);

            public Task<int> DeleteByKeyAsync<Entity>(object keyValue) => DataProvider<Entity>().DeleteByKeyAsync<Entity>(keyValue);
            public Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => DataProvider<Entity>().DeleteByKeysAsync<Entity, Key>(keys);

            #endregion

        }
    }
}