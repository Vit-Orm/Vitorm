using System;
using System.Linq;
using System.Linq.Expressions;

using Vit.Linq.ExpressionNodes;

using Vitorm.StreamQuery;

namespace Vitorm
{

    public static partial class Orm_Extensions
    {
        /// <summary>
        /// if MySql or SqlServer or Sqlite , will get sql string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [ExpressionNode_CustomMethod]
        [StreamQuery_CustomMethod]
        public static string ToExecuteString(this IQueryable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<string>(
                Expression.Call(
                    null,
                    new Func<IQueryable, string>(ToExecuteString).Method
                    , source.Expression));
        }
    }
}