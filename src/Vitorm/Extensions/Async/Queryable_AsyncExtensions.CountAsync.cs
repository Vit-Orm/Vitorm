using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Vit.Linq.ExpressionNodes;

using Vitorm.StreamQuery;

namespace Vitorm
{

    public static partial class Queryable_AsyncExtensions
    {
        [ExpressionNode_CustomMethod]
        [StreamQuery_CustomMethod]
        public static Task<int> CountAsync<T>(this IQueryable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<int>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<T>, Task<int>>(CountAsync<T>).Method
                    , source.Expression));
        }
    }





}