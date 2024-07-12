using System.Collections.Generic;

using Vitorm.DataProvider;

namespace Vitorm
{
    public partial class Data
    {
        public class DataProviderCache
        {
            public readonly IDataProvider dataProvider;
            public readonly string @namespace;
            private readonly string classFullNamePrefix;
            public readonly Dictionary<string, object> dataSourceConfig;

            public DataProviderCache(IDataProvider dataProvider, Dictionary<string, object> dataSourceConfig)
            {
                this.dataProvider = dataProvider;
                this.dataSourceConfig = dataSourceConfig;

                if (dataSourceConfig.TryGetValue("namespace", out var ns))
                {
                    @namespace = ns as string;
                    classFullNamePrefix = @namespace + ".";
                }
            }
            internal bool Match(string classFullName)
            {
                return classFullName.StartsWith(classFullNamePrefix);
            }
        }

    }
}
