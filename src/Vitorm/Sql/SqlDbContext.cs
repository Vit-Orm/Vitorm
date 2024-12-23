using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Vit.Linq;

using Vitorm.Sql.DataReader.EntityReader;
using Vitorm.Sql.SqlExecute;
using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        public SqlDbContext() : base(SqlDbSetConstructor.CreateDbSet)
        {
        }



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


        public static StreamReader defaultStreamReader = new StreamReader();
        public StreamReader streamReader = defaultStreamReader;


        public bool query_ToListAndTotalCount_InvokeInOneExecute = true;

        #endregion



    }
}
