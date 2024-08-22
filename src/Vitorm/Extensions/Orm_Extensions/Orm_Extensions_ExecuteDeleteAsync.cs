using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Vit.Linq.ExpressionNodes;

using Vitorm.StreamQuery;

namespace Vitorm
{

    public static partial class Orm_Extensions
    {
        /// <summary>
        /// delete from first collection if joined multiple collections
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [ExpressionNode_CustomMethod]
        [StreamQuery_CustomMethod]
        public static Task<int> ExecuteDeleteAsync(this IQueryable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<int>>(
                Expression.Call(
                    null,
                    new Func<IQueryable, Task<int>>(ExecuteDeleteAsync).Method
                    , source.Expression));
        }
    }
}