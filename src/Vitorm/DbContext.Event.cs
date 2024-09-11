using System;
using System.Collections.Generic;

namespace Vitorm
{
    public partial class DbContext : IDbContext, IDisposable
    {
        public static Action<ExecuteEventArgument> event_DefaultOnExecuting;
        public Action<ExecuteEventArgument> event_OnExecuting = event_DefaultOnExecuting;
    }

    public static class DbContext_Extensions_Event
    {
        public static void Event_OnExecuting(this DbContext dbContext, string executeString = null, object param = null)
        {
            if (dbContext.event_OnExecuting != null)
                dbContext.event_OnExecuting(new(dbContext, executeString, param));
        }
        public static void Event_OnExecuting(this DbContext dbContext, ExecuteEventArgument arg)
        {
            if (dbContext.event_OnExecuting != null)
                dbContext.event_OnExecuting(arg);
        }
    }

    public class ExecuteEventArgument
    {
        public ExecuteEventArgument() { }
        public ExecuteEventArgument(DbContext dbContext, string executeString = null, object param = null, Dictionary<string, object> extraParam = null)
        {
            this.dbContext = dbContext;
            this.executeString = executeString;
            this.param = param;
            this.extraParam = extraParam;
        }

        public DbContext dbContext;
        public string executeString;
        public object param;
        public Dictionary<string, object> extraParam;
        public object GetExtraParam(string key) => extraParam?.TryGetValue(key, out var value) == true ? value : null;

        public ExecuteEventArgument SetExtraParam(string key, object param)
        {
            extraParam ??= new();
            extraParam[key] = param;
            return this;
        }
    }

}
