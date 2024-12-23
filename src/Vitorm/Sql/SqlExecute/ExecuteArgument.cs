using System.Collections.Generic;
using System.Data;

namespace Vitorm.Sql.SqlExecute
{
    public class ExecuteArgument
    {
        public ExecuteArgument() { }
        public ExecuteArgument(IDbConnection connection, string text, IDictionary<string, object> parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            this.connection = connection;
            this.text = text;
            this.parameters = parameters;
            this.transaction = transaction;
            this.commandTimeout = commandTimeout;
            this.commandType = commandType;
        }


        public IDbConnection connection { get; set; }
        public string text { get; set; }
        public IDictionary<string, object> parameters { get; set; }
        public CommandType? commandType { get; set; }
        public IDbTransaction transaction { get; set; }
        public int? commandTimeout { get; set; }
    }
}
