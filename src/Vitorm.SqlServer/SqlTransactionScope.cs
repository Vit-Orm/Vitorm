using System;
using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.Transaction;

using SqlTransaction = Microsoft.Data.SqlClient.SqlTransaction;

namespace Vitorm.SqlServer
{
    public class SqlTransactionScope : Vitorm.Sql.Transaction.SqlTransactionScope
    {
        int savePointCount = 0;
        public DbTransactionWrap CreateTransactionSavePoint(IDbTransaction originalTransaction)
        {
            var savePointName = "tran" + savePointCount++;
            return new DbTransactionWrapSavePoint(originalTransaction, savePointName);
        }
        public SqlTransactionScope(SqlDbContext dbContext) : base(dbContext)
        {
        }

        public override IDbTransaction BeginTransaction()
        {
            DbTransactionWrap transactionWrap;
            IDbTransaction originalTransaction = GetCurrentTransaction();
            if (originalTransaction == null)
            {
                var dbConnection = dbContext.dbConnection;
                if (dbConnection.State != ConnectionState.Open) dbConnection.Open();
                originalTransaction = dbConnection.BeginTransaction();

                transactionWrap = new DbTransactionWrap(originalTransaction);
            }
            else
            {
                transactionWrap = CreateTransactionSavePoint(originalTransaction);
            }

            transactions.Push(transactionWrap);
            return transactionWrap;
        }

    }

    public class DbTransactionWrapSavePoint : DbTransactionWrap
    {
        public SqlTransaction sqlTran => (SqlTransaction)originalTransaction;
        string savePointName;
        public DbTransactionWrapSavePoint(IDbTransaction transaction, string savePointName) : base(transaction)
        {
            this.savePointName = savePointName;
            sqlTran.Save(savePointName);
        }

        public override void Commit()
        {
            // no need to commit savepoint for sqlserver, ref: https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlclient.sqltransaction.save

            //sqlTran.Commit(savePointName);
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
