using System;
using System.Collections.Generic;
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
        public static Task<(List<TSource> list, int totalCount)> ToListAndTotalCountAsync<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<(List<TSource> list, int totalCount)>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<TSource>, Task<(List<TSource> list, int totalCount)>>(ToListAndTotalCountAsync<TSource>).Method
                    , source.Expression));
        }
    }





}