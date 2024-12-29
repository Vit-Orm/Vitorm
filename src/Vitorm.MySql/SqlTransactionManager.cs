using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.Transaction;
using Vitorm.Transaction;

using SqlTransaction = MySqlConnector.MySqlTransaction;

namespace Vitorm.MySql
{
    public class SqlTransactionManager : Vitorm.Sql.Transaction.SqlTransactionManager
    {
        int savePointCount = 0;
        public Sql.Transaction.SqlTransaction CreateTransactionSavePoint(IDbTransaction originalTransaction)
        {
            var savePointName = "tran" + savePointCount++;
            return new DbTransactionWrapSavePoint(originalTransaction, savePointName);
        }
        public SqlTransactionManager(SqlDbContext dbContext) : base(dbContext)
        {
        }

        public override ITransaction BeginTransaction()
        {
            Sql.Transaction.SqlTransaction transaction;
            IDbTransaction originalTransaction = GetDbTransaction();
            if (originalTransaction == null)
            {
                var dbConnection = dbContext.dbConnection as MySqlConnector.MySqlConnection;
                if (dbConnection.State != ConnectionState.Open) dbConnection.Open();

                originalTransaction = dbConnection.BeginTransaction();
                dbContext.ExecuteWithTransaction("SET autocommit=0;", transaction: originalTransaction);
                transaction = new Sql.Transaction.SqlTransaction(originalTransaction);
            }
            else
            {
                transaction = CreateTransactionSavePoint(originalTransaction);
            }

            transactions.Push(transaction);
            return transaction;
        }



        public class DbTransactionWrapSavePoint : Sql.Transaction.SqlTransaction
        {
            public SqlTransaction sqlTran => (SqlTransaction)originalTransaction;
            readonly string savePointName;
            public DbTransactionWrapSavePoint(IDbTransaction transaction, string savePointName) : base(transaction)
            {
                this.savePointName = savePointName;
                sqlTran.Save(savePointName);
            }

            public override void Commit()
            {
                sqlTran.Release(savePointName);
                TransactionState = ETransactionState.Committed;
            }

            public override void Dispose()
            {
                if (TransactionState == ETransactionState.Active)
                    sqlTran.Rollback(savePointName);
                TransactionState = ETransactionState.Disposed;
            }

            public override void Rollback()
            {
                sqlTran.Rollback(savePointName);
                TransactionState = ETransactionState.RolledBack;
            }
        }
    }
}