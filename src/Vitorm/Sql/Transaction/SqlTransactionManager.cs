using System.Collections.Generic;
using System.Data;

using Vitorm.Transaction;

namespace Vitorm.Sql.Transaction
{
    public class SqlTransactionManager : ITransactionManager
    {
        public SqlTransactionManager(SqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected SqlDbContext dbContext;
        protected Stack<SqlTransaction> transactions = new();


        public virtual ITransaction BeginTransaction()
        {
            var dbConnection = dbContext.dbConnection;
            if (dbConnection.State != ConnectionState.Open) dbConnection.Open();
            var transaction = dbConnection.BeginTransaction();

            var sqlTransaction = new SqlTransaction(transaction);
            transactions.Push(sqlTransaction);
            return sqlTransaction;
        }

        public virtual void Dispose()
        {
            while (transactions?.Count > 0)
            {
                var transaction = transactions.Pop();
                if (transaction?.TransactionState != ETransactionState.Disposed)
                {
                    transaction?.Dispose();
                }
            }
            transactions = null;
        }

        public virtual IDbTransaction GetDbTransaction()
        {
            while (transactions?.Count > 0)
            {
                var tran = transactions.Peek();
                if (tran?.TransactionState == ETransactionState.Active) return tran.originalTransaction;
                transactions.Pop();
            }
            return null;
        }
    }

}
