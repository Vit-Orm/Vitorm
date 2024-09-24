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


            #region DataProvider

            readonly ConcurrentDictionary<Type, IDataProvider> entityProviderMap = new();

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

            #region  AddDataProvider 


            /// <summary>
            /// Data.Init("appsettings.Development.json")
            /// </summary>
            /// <param name="appsettingsFileName"></param>
            public virtual DataSource AddDataProviders(string appsettingsFileName)
            {
                AddDataProviders(new JsonFile(appsettingsFileName));
                return this;
            }

            public virtual DataSource AddDataProviders(JsonFile json, string configPath = "Vitorm.Data")
            {
                var dataProviderConfigs = json.GetByPath<List<Dictionary<string, object>>>(configPath);
                return AddDataProviders(dataProviderConfigs);
            }

            public virtual DataSource AddDataProviders(IEnumerable<Dictionary<string, object>> dataProviderConfigs)
            {
                var dataProviders = dataProviderConfigs?.Select(CreateDataProvider).NotNull().ToList();

                if (dataProviders?.Any() == true) providerCache.AddRange(dataProviders);

                entityProviderMap.Clear();

                return this;
            }

            /// <summary>
            /// insert to header of DataProvider list
            /// </summary>
            /// <param name="dataProviderConfig"></param>
            /// <returns></returns>
            public virtual bool InsertDataProvider(Dictionary<string, object> dataProviderConfig)
            {
                var provider = CreateDataProvider(dataProviderConfig);
                if (provider == null) return false;

                providerCache.Insert(0, provider);
                entityProviderMap.Clear();
                return true;
            }

            /// <summary>
            /// insert to tail of DataProvider list
            /// </summary>
            /// <param name="dataProviderConfig"></param>
            /// <returns></returns>
            public virtual bool AddDataProvider(Dictionary<string, object> dataProviderConfig)
            {
                var provider = CreateDataProvider(dataProviderConfig);
                if (provider == null) return false;

                providerCache.Add(provider);
                entityProviderMap.Clear();
                return true;
            }

            public virtual void ClearDataProviders(Predicate<DataProviderCache> predicate = null)
            {
                if (predicate != null)
                    providerCache.RemoveAll(predicate);
                else
                    providerCache.Clear();

                entityProviderMap.Clear();
            }
            #endregion


            #region GetDataProvider

            public virtual IDataProvider DataProvider<Entity>() => DataProvider(typeof(Entity));
            public virtual IDataProvider DataProvider(Type entityType)
            {
                return entityProviderMap.GetOrAdd(entityType, GetDataProviderFromConfig);
            }
            private IDataProvider GetDataProviderFromConfig(Type entityType)
            {
                var classFullName = entityType.FullName;
                return providerCache.FirstOrDefault(cache => cache.Match(classFullName))?.dataProvider
                    ?? throw new NotImplementedException("can not find config for type: " + classFullName);
            }

            /// <summary>
            /// nameOrNamespace:  dataProviderName or dataProviderNamespace
            /// </summary>
            /// <param name="nameOrNamespace"></param>
            /// <returns></returns>
            public virtual IDataProvider DataProvider(string nameOrNamespace)
            {
                return providerCache.FirstOrDefault(cache => cache.name == nameOrNamespace || cache.Match(nameOrNamespace))?.dataProvider;
            }

            #endregion






            #region CRUD Sync

            // #0 Schema :  TryCreateTable TryDropTable
            public virtual void TryCreateTable<Entity>() => DataProvider<Entity>().TryCreateTable<Entity>();
            public virtual void TryDropTable<Entity>() => DataProvider<Entity>().TryDropTable<Entity>();
            public virtual void Truncate<Entity>() => DataProvider<Entity>().Truncate<Entity>();


            // #1 Create :  Add AddRange
            public virtual Entity Add<Entity>(Entity entity) => DataProvider<Entity>().Add<Entity>(entity);
            public virtual void AddRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().AddRange<Entity>(entities);

            // #2 Retrieve : Get Query
            public virtual Entity Get<Entity>(object keyValue) => DataProvider<Entity>().Get<Entity>(keyValue);
            public virtual IQueryable<Entity> Query<Entity>() => DataProvider<Entity>().Query<Entity>();


            // #3 Update: Update UpdateRange
            public virtual int Update<Entity>(Entity entity) => DataProvider<Entity>().Update<Entity>(entity);
            public virtual int UpdateRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().UpdateRange<Entity>(entities);


            // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            public virtual int Delete<Entity>(Entity entity) => DataProvider<Entity>().Delete<Entity>(entity);
            public virtual int DeleteRange<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().DeleteRange<Entity>(entities);

            public virtual int DeleteByKey<Entity>(object keyValue) => DataProvider<Entity>().DeleteByKey<Entity>(keyValue);
            public virtual int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => DataProvider<Entity>().DeleteByKeys<Entity, Key>(keys);

            #endregion



            #region CRUD Async

            // #0 Schema :  TryCreateTable TryDropTable
            public virtual Task TryCreateTableAsync<Entity>() => DataProvider<Entity>().TryCreateTableAsync<Entity>();
            public virtual Task TryDropTableAsync<Entity>() => DataProvider<Entity>().TryDropTableAsync<Entity>();
            public virtual Task TruncateAsync<Entity>() => DataProvider<Entity>().TruncateAsync<Entity>();


            // #1 Create :  Add AddRange
            public virtual Task<Entity> AddAsync<Entity>(Entity entity) => DataProvider<Entity>().AddAsync<Entity>(entity);
            public virtual Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().AddRangeAsync<Entity>(entities);

            // #2 Retrieve : Get Query
            public virtual Task<Entity> GetAsync<Entity>(object keyValue) => DataProvider<Entity>().GetAsync<Entity>(keyValue);


            // #3 Update: Update UpdateRange
            public virtual Task<int> UpdateAsync<Entity>(Entity entity) => DataProvider<Entity>().UpdateAsync<Entity>(entity);
            public virtual Task<int> UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().UpdateRangeAsync<Entity>(entities);


            // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            public virtual Task<int> DeleteAsync<Entity>(Entity entity) => DataProvider<Entity>().DeleteAsync<Entity>(entity);
            public virtual Task<int> DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) => DataProvider<Entity>().DeleteRangeAsync<Entity>(entities);

            public virtual Task<int> DeleteByKeyAsync<Entity>(object keyValue) => DataProvider<Entity>().DeleteByKeyAsync<Entity>(keyValue);
            public virtual Task<int> DeleteByKeysAsync<Entity, Key>(IEnumerable<Key> keys) => DataProvider<Entity>().DeleteByKeysAsync<Entity, Key>(keys);

            #endregion

        }
    }
}