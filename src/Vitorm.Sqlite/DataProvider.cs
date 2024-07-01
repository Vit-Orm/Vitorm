using System.Collections.Generic;

using Vit.Extensions.Vitorm_Extensions;

using Vitorm.DataProvider;
using Vitorm.Sql;

namespace Vitorm.Sqlite
{

    public class DataProvider : SqlDataProvider
    {
        protected Dictionary<string, object> config;
        protected string connectionString;
        protected int? commandTimeout;

        public override SqlDbContext CreateDbContext() => new SqlDbContext().UseSqlite(connectionString: connectionString, commandTimeout: commandTimeout);

        public override void Init(Dictionary<string, object> config)
        {
            this.config = config;
            if (config.TryGetValue("connectionString", out var connStr))
                this.connectionString = connStr as string;

            if (config.TryGetValue("commandTimeout", out var strCommandTimeout) && int.TryParse("" + strCommandTimeout, out var commandTimeout))
                this.commandTimeout = commandTimeout;
        }
    }
}
