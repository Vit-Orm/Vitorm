using Dapper;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Vit.Orm.Entity;

namespace Vit.Orm.Sql
{
    public class SqlDbContext : DbContext
    {
        protected Func<IDbConnection> createDbConnection { get; set; }
        protected IDbConnection _dbConnection;
        public override void Dispose()
        {
            base.Dispose();

            // dispose transactions
            DisposeTransactions();

            _dbConnection?.Dispose();
            _dbConnection = null;
        }
        public virtual IDbConnection dbConnection => _dbConnection ??= createDbConnection();


        public ISqlTranslator sqlTranslator { get; private set; }


        public void Init(ISqlTranslator sqlTranslator, Func<IDbConnection> createDbConnection, Func<Type, IEntityDescriptor> getEntityDescriptor)
        {
            this.sqlTranslator = sqlTranslator;
            this.createDbConnection = createDbConnection;

            this.dbSetCreator = (entityType) =>
            {
                var entityDescriptor = getEntityDescriptor(entityType);
                return SqlDbSetConstructor.CreateDbSet(this, entityType, entityDescriptor);
            };
        }

        protected Stack<TransactionWrap> transactions;
        protected IDbTransaction GetCurrentTransaction()
        {
            if (transactions == null) return null;

            while (transactions.Count > 0)
            {
                var tran = transactions.Peek();
                if (tran?.TransactionState == TransactionWrap.ETransactionState.Active) return tran.originalTransaction;
                transactions.Pop();
            }
            return null;
        }
        protected void DisposeTransactions()
        {
            if (transactions == null) return;

            while (transactions.Count > 0)
            {
                var transaction = transactions.Pop();
                if (transaction?.TransactionState != TransactionWrap.ETransactionState.Disposed)
                {
                    transaction?.Dispose();
                }
            }
            transactions = null;
        }
        public override IDbTransaction BeginTransaction()
        {
            if (dbConnection.State != ConnectionState.Open) dbConnection.Open();

            var transaction = dbConnection.BeginTransaction();

            transactions ??= new();
            var wrap = new TransactionWrap(transaction);
            transactions.Push(wrap);
            return wrap;
        }

        public class TransactionWrap : IDbTransaction
        {
            public enum ETransactionState
            {
                Active, Committed, RolledBack, Disposed
            }
            public ETransactionState TransactionState { get; private set; } = ETransactionState.Active;
            public TransactionWrap(IDbTransaction transaction)
            {
                originalTransaction = transaction;
            }
            public IDbTransaction originalTransaction;

            public IDbConnection Connection => originalTransaction.Connection;

            public System.Data.IsolationLevel IsolationLevel => originalTransaction.IsolationLevel;

            public void Commit()
            {
                originalTransaction.Commit();
                TransactionState = ETransactionState.Committed;
            }

            public void Dispose()
            {
                originalTransaction.Dispose();
                TransactionState = ETransactionState.Disposed;
            }

            public void Rollback()
            {
                originalTransaction.Rollback();
                TransactionState = ETransactionState.RolledBack;
            }
        }



        #region Execute

        public int commandTimeout = 0;

        public virtual int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var transaction = GetCurrentTransaction();
            commandTimeout ??= this.commandTimeout;
            return dbConnection.Execute(sql, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        public virtual IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var transaction = GetCurrentTransaction();
            commandTimeout ??= this.commandTimeout;
            return dbConnection.ExecuteReader(sql, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        public virtual object ExecuteScalar(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var transaction = GetCurrentTransaction();
            commandTimeout ??= this.commandTimeout;
            return dbConnection.ExecuteScalar(sql, param: param, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

    }
}
