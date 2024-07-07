using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql.DataReader
{
    public class SqlColumns
    {
        List<Column> columns = new();

        /// <summary>
        /// entity field , try get sql column and return sqlColumnIndex
        /// </summary>
        /// <param name="sqlTranslator"></param>
        /// <param name="tableName"></param>
        /// <param name="columnDescriptor"></param>
        /// <returns></returns>
        public int AddSqlColumnAndGetIndex(ISqlTranslateService sqlTranslator, string tableName, IColumnDescriptor columnDescriptor)
        {
            var sqlColumnName = sqlTranslator.GetSqlField(tableName, columnDescriptor.columnName);

            var sqlColumnIndex = columns.FirstOrDefault(m => m.sqlColumnName == sqlColumnName)?.sqlColumnIndex ?? -1;
            if (sqlColumnIndex < 0)
            {
                sqlColumnIndex = columns.Count;
                columns.Add(new Column { tableName = tableName, columnDescriptor = columnDescriptor, sqlColumnName = sqlColumnName, sqlColumnAlias = "c" + sqlColumnIndex, sqlColumnIndex = sqlColumnIndex });
            }
            return sqlColumnIndex;
        }

        /// <summary>
        ///  aggregate column in GroupBy
        /// </summary>
        /// <param name="sqlColumnSentence"> for example:   Sum([t0].[userId])  ,  [t0].[userFatherId]  </param>
        /// <returns></returns>
        public int AddSqlColumnAndGetIndex(string sqlColumnSentence)
        {
            var sqlColumnName = sqlColumnSentence;

            var sqlColumnIndex = columns.FirstOrDefault(m => m.sqlColumnName == sqlColumnName)?.sqlColumnIndex ?? -1;
            if (sqlColumnIndex < 0)
            {
                sqlColumnIndex = columns.Count;
                columns.Add(new Column { sqlColumnName = sqlColumnName, sqlColumnAlias = "c" + sqlColumnIndex, sqlColumnIndex = sqlColumnIndex });
            }
            return sqlColumnIndex;
        }

        /// <summary>
        ///  alias table column  (  users.Select(u=> new { u.id } )   )
        /// </summary>
        /// <param name="sqlTranslator"></param>
        /// <param name="member"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public int AddSqlColumnAndGetIndex(ISqlTranslateService sqlTranslator, ExpressionNode_Member member, DbContext dbContext)
        {
            var sqlColumnName = sqlTranslator.GetSqlField(member, dbContext);

            var sqlColumnIndex = columns.FirstOrDefault(m => m.sqlColumnName == sqlColumnName)?.sqlColumnIndex ?? -1;
            if (sqlColumnIndex < 0)
            {
                sqlColumnIndex = columns.Count;
                columns.Add(new Column { member = member, sqlColumnName = sqlColumnName, sqlColumnAlias = "c" + sqlColumnIndex, sqlColumnIndex = sqlColumnIndex });
            }
            return sqlColumnIndex;
        }


        public string GetSqlColumns()
        {
            var sqlColumns = columns.Select(column => column.sqlColumnName + " as " + column.sqlColumnAlias);
            return String.Join(", ", sqlColumns);
        }

        public string GetColumnAliasBySqlColumnName(string sqlColumnName)
        {
            return columns.FirstOrDefault(col => col.sqlColumnName == sqlColumnName)?.sqlColumnAlias;
        }

        class Column
        {
            // or table alias
            public string tableName;
            public IColumnDescriptor columnDescriptor;
            public ExpressionNode_Member member;

            public string sqlColumnName;
            public string sqlColumnAlias;

            public int sqlColumnIndex;
        }

    }



}
