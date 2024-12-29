using System.Data;

using Vitorm.Transaction;

namespace Vitorm.Sql.Transaction
{
    public partial class SqlTransaction : ITransaction
    {
        public virtual ETransactionState TransactionState { get; protected set; } = ETransactionState.Active;
        public SqlTransaction(IDbTransaction transaction)
        {
            originalTransaction = transaction;
        }
        public IDbTransaction originalTransaction;

        //public virtual IDbConnection Connection => originalTransaction.Connection;

        //public virtual System.Data.IsolationLevel IsolationLevel => originalTransaction.IsolationLevel;

        public virtual void Commit()
        {
            originalTransaction.Commit();
            TransactionState = ETransactionState.Committed;
        }
        public virtual void Dispose()
        {
            originalTransaction.Dispose();
            TransactionState = ETransactionState.Disposed;
        }

        public virtual void Rollback()
        {
            originalTransaction.Rollback();
            TransactionState = ETransactionState.RolledBack;
        }
    }

}
