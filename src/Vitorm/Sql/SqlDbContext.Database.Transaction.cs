using System;
using System.Data;

using Vitorm.Sql.Transaction;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

        #region Transaction
        public virtual Func<SqlDbContext, ITransactionScope> createTransactionScope { set; get; }
                    = (dbContext) => new SqlTransactionScope(dbContext);
        protected virtual ITransactionScope transactionScope { get; set; }

        public virtual IDbTransaction BeginTransaction()
        {
            transactionScope ??= createTransactionScope(this);
            return transactionScope.BeginTransaction();
        }
        public virtual IDbTransaction GetCurrentTransaction() => transactionScope?.GetCurrentTransaction();

        #endregion
    }
}
