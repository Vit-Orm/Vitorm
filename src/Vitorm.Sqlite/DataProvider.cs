using System.Collections.Generic;

using Vitorm.DataProvider;
using Vitorm.Sql;

namespace Vitorm.Sqlite
{
    public class DataProvider : SqlDataProvider
    {
        protected Dictionary<string, object> config;
        protected DbConfig dbConfig;

        public override SqlDbContext CreateDbContext() => new SqlDbContext().UseSqlite(dbConfig);

        public override void Init(Dictionary<string, object> config)
        {
            this.config = config;
            this.dbConfig = new(config);
        }
    }
}
