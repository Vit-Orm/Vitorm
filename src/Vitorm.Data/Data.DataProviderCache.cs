using System.Collections.Generic;
using System.Linq;

using Vitorm.DataProvider;

namespace Vitorm
{
    public partial class Data
    {
        public class DataProviderCache
        {
            public readonly IDataProvider dataProvider;
            /// <summary>
            /// DataProviderName ( will be namespace if null)
            /// </summary>
            public readonly string name;
            /// <summary>
            /// separate by comma, for example: "Vitorm.Model.Mysql,Vitorm.Model.Sqlserver"
            /// </summary>
            public readonly string @namespace;
            private readonly List<string> classFullNamePrefixList;
            public readonly Dictionary<string, object> dataSourceConfig;

            public DataProviderCache(IDataProvider dataProvider, Dictionary<string, object> dataSourceConfig)
            {
                this.dataProvider = dataProvider;
                this.dataSourceConfig = dataSourceConfig;

                if (dataSourceConfig.TryGetValue("namespace", out var ns) && ns is string strNs)
                {
                    name = @namespace = strNs;
                }

                if (dataSourceConfig.TryGetValue("name", out var n) && n is string strName)
                {
                    name = strName;
                }

                classFullNamePrefixList = @namespace?.Split(',').Select(ns => ns + ".").ToList();
            }
            internal bool Match(string classFullName)
            {
                return classFullNamePrefixList?.Any(classFullNamePrefix => classFullName.StartsWith(classFullNamePrefix)) == true;
            }
        }

    }
}
