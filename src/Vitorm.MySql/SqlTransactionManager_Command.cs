using System.Collections.Generic;
using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.Transaction;
using Vitorm.Transaction;


namespace Vitorm.MySql
{
    /*
           // ref: https://dev.mysql.com/doc/refman/8.4/en/savepoint.html
           //  https://dev.mysql.com/doc/refman/8.4/en/commit.html

          START TRANSACTION;
              SET autocommit=0;
              SAVEPOINT tran0;
                  select '';
              -- ROLLBACK WORK TO SAVEPOINT tran0;
              RELEASE SAVEPOINT tran0;
          COMMIT;
          -- ROLLBACK;
    */
    public class SqlTransactionManager_Command : ITransactionManager
    {
        protected SqlDbContext dbContext;
        protected Stack<TransactionSavePoint> savePoints = new();
        int savePointCount = 0;
        public SqlTransactionManager_Command(SqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        Transaction_Command transaction;
        public virtual ITransaction BeginTransaction()
        {
            if (transaction == null)
            {
                var dbConnection = dbContext.dbConnection;
                if (dbConnection.State != ConnectionState.Open) dbConnection.Open();

                transaction = new Transaction_Command(dbContext);
                return transaction;

            }
            var savePointName = "tran" + savePointCount++;
            var savePoint = transaction.BeginSavePoint(savePointName);

            savePoints.Push(savePoint);
            return savePoint;
        }
        public virtual void Dispose()
        {
            while (savePoints?.Count > 0)
            {
                var transaction = savePoints.Pop();
                if (transaction?.TransactionState != ETransactionState.Disposed)
                {
                    transaction?.Dispose();
                }
            }
            savePoints = null;

            transaction?.Dispose();
            transaction = null;
        }



        public class Transaction_Command : ITransaction
        {
            public virtual System.Data.IsolationLevel IsolationLevel => default;
            public IDbConnection Connection => dbContext.dbConnection;
            readonly SqlDbContext dbContext;
            public virtual ETransactionState TransactionState { get; protected set; } = ETransactionState.Active;

            public Transaction_Command(SqlDbContext dbContext)
            {
                this.dbContext = dbContext;
                Execute($"START TRANSACTION; SET autocommit=0;");
            }

            public void Commit()
            {
                Execute($"COMMIT;");
                TransactionState = ETransactionState.Committed;
            }

            public void Dispose()
            {
                if (TransactionState == ETransactionState.Active)
                {
                    Execute($"ROLLBACK;");
                }
                TransactionState = ETransactionState.Disposed;
            }

            public void Rollback()
            {
                Execute($"ROLLBACK;");
                TransactionState = ETransactionState.RolledBack;
            }
            public TransactionSavePoint BeginSavePoint(string savePoint)
            {
                return new TransactionSavePoint(dbContext, savePoint);
            }
            protected virtual void Execute(string sql)
            {
                dbContext.ExecuteWithTransaction(sql);
            }
        }

        public class TransactionSavePoint : ITransaction
        {
            public virtual System.Data.IsolationLevel IsolationLevel => default;

            public IDbConnection Connection => dbContext.dbConnection;
            readonly SqlDbContext dbContext;
            public virtual ETransactionState TransactionState { get; protected set; } = ETransactionState.Active;
            protected string savePointName;


            protected virtual void Execute(string sql)
            {
                dbContext.ExecuteWithTransaction(sql);
            }
            public TransactionSavePoint(SqlDbContext dbContext, string savePointName)
            {
                this.dbContext = dbContext;
                this.savePointName = savePointName;
                Execute($"SAVEPOINT {savePointName};");
            }

            public void Commit()
            {
                Execute($"RELEASE SAVEPOINT {savePointName};");
                TransactionState = ETransactionState.Committed;
            }

            public void Dispose()
            {
                if (TransactionState == ETransactionState.Active)
                {
                    Execute($"ROLLBACK WORK TO SAVEPOINT {savePointName};");
                }
                TransactionState = ETransactionState.Disposed;
            }

            public void Rollback()
            {
                Execute($"ROLLBACK WORK TO SAVEPOINT {savePointName};");
                TransactionState = ETransactionState.RolledBack;
            }
        }
    }
}