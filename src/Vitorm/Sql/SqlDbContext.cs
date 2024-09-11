using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

using Vit.Linq;

using Vitorm.Entity;
using Vitorm.Sql.DataReader.EntityReader;
using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        public SqlDbContext() : base(DefaultDbSetCreator)
        {
        }

        #region DbSet
        static IDbSet DefaultDbSetCreator(IDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return SqlDbSetConstructor.CreateDbSet(dbContext, entityDescriptor);
        }

        #endregion



        #region EntityReader


        /// <summary>
        /// Vitorm.Sql.DataReader.EntityReader.IEntityReader
        /// 
        /// SqlDbContext.defaultEntityReaderType = typeof(global::Vitorm.Sql.DataReader.EntityReader.EntityConstructor.EntityReader) ;  // default
        /// SqlDbContext.defaultEntityReaderType = typeof(global::Vitorm.Sql.DataReader.EntityReader.CompiledLambda.EntityReader) ;
        /// </summary>
        public static Type defaultEntityReaderType = typeof(Vitorm.Sql.DataReader.EntityReader.EntityConstructor.EntityReader);
        public Type entityReaderType { get; protected set; } = defaultEntityReaderType;

        /// <summary>
        ///   SetEntityReader<global::Vitorm.Sql.DataReader.EntityReader.EntityConstructor.EntityReader>();  // default
        ///   SetEntityReader<global::Vitorm.Sql.DataReader.EntityReader.CompiledLambda.EntityReader>();
        /// </summary>
        /// <typeparam name="EntityReader"></typeparam>
        public virtual void SetEntityReader<EntityReader>()
            where EntityReader : IEntityReader, new()
        {
            entityReaderType = typeof(EntityReader);
        }
        #endregion


        #region dbConnection
        protected DbConnectionProvider dbConnectionProvider;
        protected IDbConnection _dbConnection;
        protected IDbConnection _readOnlyDbConnection;
        public override void Dispose()
        {
            try
            {
                transactionScope?.Dispose();
            }
            finally
            {
                transactionScope = null;
                try
                {
                    _dbConnection?.Dispose();
                }
                finally
                {
                    _dbConnection = null;

                    try
                    {
                        _readOnlyDbConnection?.Dispose();
                    }
                    finally
                    {
                        _readOnlyDbConnection = null;

                        base.Dispose();
                    }
                }
            }
        }
        public virtual IDbConnection dbConnection => _dbConnection ??= dbConnectionProvider.CreateDbConnection();
        public virtual IDbConnection readOnlyDbConnection
            => _readOnlyDbConnection ??
                (dbConnectionProvider.ableToCreateReadOnly ? (_readOnlyDbConnection = dbConnectionProvider.CreateReadOnlyDbConnection()) : dbConnection);

        /// <summary>
        /// to identify whether contexts are from the same database
        /// </summary>
        public virtual string dbGroupName => "SqlDbSet_" + dbConnectionProvider.dbHashCode;
        public virtual string databaseName => dbConnectionProvider.databaseName;

        public virtual void ChangeDatabase(string databaseName)
        {
            if (_dbConnection != null || _readOnlyDbConnection != null) throw new InvalidOperationException("can not change database after connected, please try in an new DbContext.");

            dbConnectionProvider = dbConnectionProvider.WithDatabase(databaseName);
        }

        #endregion

        public virtual ISqlTranslateService sqlTranslateService { get; private set; }

        public virtual void Init(ISqlTranslateService sqlTranslateService, DbConnectionProvider dbConnectionProvider, SqlExecutor sqlExecutor = null, Dictionary<string, object> extraConfig = null)
        {
            this.sqlTranslateService = sqlTranslateService;
            this.dbConnectionProvider = dbConnectionProvider;
            this.sqlExecutor = sqlExecutor ?? SqlExecutor.Instance;

            extraConfig?.ForEach(kv =>
            {
                switch (kv.Key)
                {
                    case nameof(query_ToListAndTotalCount_InvokeInOneExecute):
                        {
                            if (kv.Value is bool invokeInOneExecute) query_ToListAndTotalCount_InvokeInOneExecute = invokeInOneExecute;
                            break;
                        }
                }
            });
        }


        #region Query
        public Action<SqlDbContext, Expression, Type, object> AfterQuery;
        public virtual SqlDbContext AutoDisposeAfterQuery()
        {
            AfterQuery += (_, _, _, _) => Dispose();
            return this;
        }


        #region StreamReader
        public static StreamReader defaultStreamReader = new StreamReader();
        public StreamReader streamReader = defaultStreamReader;
        #endregion

        public bool query_ToListAndTotalCount_InvokeInOneExecute = true;



        #endregion



    }
}
