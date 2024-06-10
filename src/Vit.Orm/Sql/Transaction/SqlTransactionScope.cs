using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Vit.Orm.Sql.Transaction
{
    public class SqlTransactionScope : ITransactionScope
    {
        public SqlTransactionScope(SqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected SqlDbContext dbContext;
        protected Stack<DbTransactionWrap> transactions = new();


        public virtual IDbTransaction BeginTransaction()
        {
            var dbConnection = dbContext.dbConnection;
            if (dbConnection.State != ConnectionState.Open) dbConnection.Open();
            var transaction = dbConnection.BeginTransaction();

            var transactionWrap = new DbTransactionWrap(transaction);
            transactions.Push(transactionWrap);
            return transactionWrap;
        }

        public virtual void Dispose()
        {
            while (transactions?.Count > 0)
            {
                var transaction = transactions.Pop();
                if (transaction?.TransactionState != DbTransactionWrap.ETransactionState.Disposed)
                {
                    transaction?.Dispose();
                }
            }
            transactions = null;
        }

        public virtual IDbTransaction GetCurrentTransaction()
        {
            while (transactions?.Count > 0)
            {
                var tran = transactions.Peek();
                if (tran?.TransactionState == DbTransactionWrap.ETransactionState.Active) return tran.originalTransaction;
                transactions.Pop();
            }
            return null;
        }
    }

}
