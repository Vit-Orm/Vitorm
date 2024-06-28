using Vitorm.Sql;
using Vit.Extensions;
using Vitorm.DataProvider;
using System.Collections.Generic;

namespace Vitorm.Sqlite
{

    public class DataProvider : SqlDataProvider
    {
        protected Dictionary<string, object> config;
        protected string connectionString;

        public override SqlDbContext CreateDbContext() => new SqlDbContext().UseSqlite(connectionString);

        public override void Init(Dictionary<string, object> config)
        {
            this.config = config;
            if (config.TryGetValue("connectionString", out var connStr))
                this.connectionString = connStr as string;
        }
    }
}
