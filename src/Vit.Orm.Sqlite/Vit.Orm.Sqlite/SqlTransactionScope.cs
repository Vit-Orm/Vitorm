using System;
using System.Data;

using Vit.Orm.Sql;
using Vit.Orm.Sql.Transaction;

using SqlTransaction = Microsoft.Data.Sqlite.SqliteTransaction;

namespace Vit.Orm.Sqlite
{
    public class SqlTransactionScope : Vit.Orm.Sql.Transaction.SqlTransactionScope
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
        string savePoint;
        public DbTransactionWrapSavePoint(IDbTransaction transaction, string savePoint) : base(transaction)
        {
            this.savePoint = savePoint;
            sqlTran.Save(savePoint);
        }

        public override void Commit()
        {
            sqlTran.Release(savePoint);
            TransactionState = ETransactionState.Committed;
        }

        public override void Dispose()
        {
            if (TransactionState == ETransactionState.Active)
                sqlTran.Rollback(savePoint);
            TransactionState = ETransactionState.Disposed;
        }

        public override void Rollback()
        {
            sqlTran.Rollback(savePoint);
            TransactionState = ETransactionState.RolledBack;
        }
    }
}
