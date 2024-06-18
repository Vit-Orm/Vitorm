using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.Transaction;
using System.Collections.Generic;
using static Vitorm.Sql.Transaction.DbTransactionWrap;

namespace Vitorm.MySql
{
    public class SqlTransactionScope_Command : ITransactionScope
    {
        protected SqlDbContext dbContext;
        protected Stack<DbTransactionWrapSavePoint> savePoints = new();
        int savePointCount = 0;
        public SqlTransactionScope_Command(SqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        DbTransactionWrap_Command dbTransactionWrap;
        public virtual IDbTransaction BeginTransaction()
        {
            if (dbTransactionWrap == null)
            {
                var dbConnection = dbContext.dbConnection;
                if (dbConnection.State != ConnectionState.Open) dbConnection.Open();

                dbTransactionWrap = new DbTransactionWrap_Command(dbContext);
                return dbTransactionWrap;

            }
            var savePointName = "tran" + savePointCount++;
            var savePoint = dbTransactionWrap.BeginSavePoint(savePointName);

            savePoints.Push(savePoint);
            return savePoint;
        }

        public virtual IDbTransaction GetCurrentTransaction() => null;

        public virtual void Dispose()
        {
            while (savePoints?.Count > 0)
            {
                var transaction = savePoints.Pop();
                if (transaction?.TransactionState != DbTransactionWrap.ETransactionState.Disposed)
                {
                    transaction?.Dispose();
                }
            }
            savePoints = null;

            dbTransactionWrap?.Dispose();
            dbTransactionWrap = null;
        }



        public class DbTransactionWrap_Command : IDbTransaction
        {
            public virtual System.Data.IsolationLevel IsolationLevel => default;
            public IDbConnection Connection => dbContext.dbConnection;
            SqlDbContext dbContext;
            public virtual ETransactionState TransactionState { get; protected set; } = ETransactionState.Active;

            public DbTransactionWrap_Command(SqlDbContext dbContext)
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
            public DbTransactionWrapSavePoint BeginSavePoint(string savePoint)
            {
                return new DbTransactionWrapSavePoint(dbContext, savePoint);
            }
            protected virtual void Execute(string sql)
            {
                dbContext.ExecuteWithTransaction(sql);
            }
        }

        public class DbTransactionWrapSavePoint : IDbTransaction
        {
            public virtual System.Data.IsolationLevel IsolationLevel => default;

            public IDbConnection Connection => dbContext.dbConnection;
            SqlDbContext dbContext;
            public virtual ETransactionState TransactionState { get; protected set; } = ETransactionState.Active;
            protected string savePointName;


            protected virtual void Execute(string sql)
            {
                dbContext.ExecuteWithTransaction(sql);
            }
            public DbTransactionWrapSavePoint(SqlDbContext dbContext, string savePointName)
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