using System;
using System.Collections.Generic;

namespace Vitorm
{
    public partial class DbContext : IDbContext, IDisposable
    {
        public static Action<ExecuteEventArgument> event_DefaultOnExecuting;
        public Action<ExecuteEventArgument> event_OnExecuting = event_DefaultOnExecuting;

        public virtual void Event_OnExecuting(string executeString = null, object param = null)
        {
            event_OnExecuting?.Invoke(new(this, executeString, param));
        }

        public virtual void Event_OnExecuting(ExecuteEventArgument arg)
        {
            event_OnExecuting?.Invoke(arg);
        }

        public virtual void Event_OnExecuting(Lazy<ExecuteEventArgument> arg)
        {
            event_OnExecuting?.Invoke(new ExecuteEventArgument_Lazy(arg));
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
            this._extraParam = extraParam;
        }


        public virtual DbContext dbContext { get; protected set; }
        public virtual string executeString { get; protected set; }
        public virtual object param { get; protected set; }

        protected Dictionary<string, object> _extraParam;
        public virtual Dictionary<string, object> extraParam { get => _extraParam; set => _extraParam = value; }
        public virtual object GetExtraParam(string key) => extraParam?.TryGetValue(key, out var value) == true ? value : null;

        public ExecuteEventArgument SetExtraParam(string key, object param)
        {
            extraParam ??= new();
            extraParam[key] = param;
            return this;
        }
    }

    public class ExecuteEventArgument_Lazy : ExecuteEventArgument
    {
        protected Lazy<ExecuteEventArgument> _lazyArg;

        public ExecuteEventArgument_Lazy(Lazy<ExecuteEventArgument> lazyArg)
        {
            this._lazyArg = lazyArg;
        }

        public override DbContext dbContext => _lazyArg.Value?.dbContext;
        public override string executeString => _lazyArg.Value?.executeString;
        public override object param => _lazyArg.Value?.dbContext;
        public override Dictionary<string, object> extraParam
        {
            get
            {
                return _lazyArg.Value?.extraParam;
            }
            set
            {
                _lazyArg.Value.extraParam = value;
            }
        }

    }

}
