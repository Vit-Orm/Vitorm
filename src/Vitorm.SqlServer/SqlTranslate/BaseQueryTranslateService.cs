using System;
using System.Linq;

using Vitorm.Sql.DataReader;
using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.SqlServer.SqlTranslate
{

    public abstract class BaseQueryTranslateService : Vitorm.Sql.SqlTranslate.BaseQueryTranslateService
    {
        public BaseQueryTranslateService(SqlTranslateService sqlTranslator) : base(sqlTranslator)
        {
        }


        /*
SELECT [t].[id], [t].[birth], [t].[fatherId], [t].[motherId], [t].[name]
FROM (
    SELECT [m].[id], [m].[birth], [m].[fatherId], [m].[motherId], [m].[name], ROW_NUMBER() OVER(ORDER BY [m].[fatherId], [m].[motherId] DESC) AS [__RowNumber__]
    FROM [User] AS [m]
    WHERE [m].[id] <> 2
) AS [t]
WHERE ([t].[__RowNumber__] > 1) AND ([t].[__RowNumber__] <= 13);

// if no orders:
ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]

         */

        public override string BuildQuery(QueryTranslateArgument arg, CombinedStream stream)
        {
            if (stream.skip > 0)
            {
                if (stream.distinct != true)
                {
                    return BuildQueryWithSkip(arg, stream);
                }
                else
                {
                    string sql;

                    var (skip, take, orders) = (stream.skip.Value, stream.take, stream.orders);
                    (stream.skip, stream.take, stream.orders) = (null, null, null);

                    var innerQuery = BuildQueryWithoutSkip(arg, stream);

                    (stream.skip, stream.take, stream.orders) = (skip, take, orders);


                    #region order by
                    string sqlRowNumber;
                    if (stream.orders?.Any() == true)
                    {
                        var sqlColumns = ((DataReader)arg.dataReader).sqlColumns;
                        var orderBy = ReadOrderBy(arg, stream);
                        sqlRowNumber = $" ROW_NUMBER() OVER(ORDER BY {orderBy}) AS [__RowNumber__]";


                        string ReadOrderBy(QueryTranslateArgument arg, CombinedStream stream)
                        {
                            var columns = stream.orders.Select(field =>
                                {
                                    var sqlColumnName = sqlTranslator.EvalExpression(arg, field.member);
                                    var sqlColumnAlias = sqlColumns.GetColumnAliasBySqlColumnName(sqlColumnName);
                                    if (sqlColumnAlias == null) throw new NotSupportedException("can not find sort column from result columns, sort column name:" + sqlColumnName);
                                    return sqlColumnAlias + " " + (field.asc ? "asc" : "desc");
                                }
                            );
                            return String.Join(", ", columns);
                        }
                    }
                    else
                    {
                        sqlRowNumber = $" ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]";
                    }
                    #endregion

                    #region page inner query
                    /*
SELECT *
FROM (
    SELECT *, ROW_NUMBER(ORDER BY @@RowCount) AS [__RowNumber__]
    FROM ({innerQuery}) as [t]
) AS [t]
WHERE ([t].[__RowNumber__] > 1) AND ([t].[__RowNumber__] <= 13);
 */
                    sql = $@"
                    SELECT *
                    FROM (
                        SELECT *, {sqlRowNumber}
                        FROM ({innerQuery}) as [t]
                    ) AS [t]
                    WHERE [t].[__RowNumber__] > {stream.skip} {(stream.take > 0 ? "AND [t].[__RowNumber__] <= " + (stream.take + stream.skip) : "")} ;
                    ";
                    #endregion

                    return sql;
                }
            }
            else
            {
                return BuildQueryWithoutSkip(arg,stream);
            }
        }

        public virtual string BuildQueryWithSkip(QueryTranslateArgument arg, CombinedStream stream) 
        {
            // ROW_NUMBER() OVER(ORDER BY [m].[fatherId], [m].[motherId] DESC) AS [__RowNumber__]

            string sql = "";


            #region #0 select
            sql += ReadSelect(arg, stream);

            if (stream.orders?.Any() == true)
            {
                var orderBy = ReadOrderBy(arg, stream);
                sql += $", ROW_NUMBER() OVER(ORDER BY {orderBy}) AS [__RowNumber__]";
            }
            else
            {
                sql += $", ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]";
            }
            #endregion


            #region #1 from
            sql += "\r\n from " + ReadInnerTable(arg, stream.source);
            #endregion

            #region #2 join
            if (stream.joins != null)
            {
                sql += ReadJoin(arg, stream);
            }
            #endregion

            // #3 where 1=1
            if (stream.where != null)
            {
                var where = sqlTranslator.EvalExpression(arg, stream.where);
                if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n where " + where;
            }

            #region #4 group by
            if (stream.groupByFields != null)
            {
                sql += "\r\n group by " + ReadGroupBy(arg, stream);
            }
            #endregion

            #region #5 having
            if (stream.having != null)
            {
                var where = sqlTranslator.EvalExpression(arg, stream.having);
                if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n having " + where;
            }
            #endregion


            #region #6 Range
            /*
SELECT *
FROM (
    SELECT *, ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
    FROM [User] AS [m]
    WHERE [m].[id] <> 2
) AS [t]
WHERE ([t].[__RowNumber__] > 1) AND ([t].[__RowNumber__] <= 13);
             */
            return $@"
SELECT *
FROM (
    {sql}
) AS [t]
WHERE [t].[__RowNumber__] > {stream.skip} {(stream.take.HasValue ? "AND [t].[__RowNumber__] <= " + (stream.take + stream.skip) : "")} ;
";
            #endregion
        }



        public virtual string BuildQueryWithoutSkip(QueryTranslateArgument arg, CombinedStream stream)
        {
            // "select * "   or   "select top 10 * "

            string sql = "select ";

            // #0  select
            if (stream.take.HasValue) sql += "top " + stream.take + " ";
            sql += ReadSelect(arg, stream, prefix: null);


            #region #1 from
            sql += "\r\n from " + ReadInnerTable(arg, stream.source);
            #endregion

            #region #2 join
            if (stream.joins != null)
            {
                sql += ReadJoin(arg, stream);
            }
            #endregion

            // #3 where 1=1
            if (stream.where != null)
            {
                var where = sqlTranslator.EvalExpression(arg, stream.where);
                if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n where " + where;
            }

            #region #4 group by
            if (stream.groupByFields != null)
            {
                sql += "\r\n group by " + ReadGroupBy(arg, stream);
            }
            #endregion

            #region #5 having
            if (stream.having != null)
            {
                var where = sqlTranslator.EvalExpression(arg, stream.having);
                if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n having " + where;
            }
            #endregion


            // #6 OrderBy
            if (stream.orders?.Any() == true)
            {
                sql += "\r\n order by " + ReadOrderBy(arg, stream);
            }

            return sql;
        }



    }

}
