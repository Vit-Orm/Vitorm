using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Vit.Linq.ExpressionNodes.ExpressionConvertor.MethodCalls;

using Vitorm.StreamQuery;

namespace Vitorm
{

    public static partial class Queryable_AsyncExtensions
    {
        [ExpressionNode_CustomMethod]
        [StreamQuery_CustomMethod]
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<TSource>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<TSource>, Task<TSource>>(FirstOrDefaultAsync<TSource>).Method
                    , source.Expression));
        }
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
            => source?.Where(predicate).FirstOrDefaultAsync();


        [ExpressionNode_CustomMethod]
        [StreamQuery_CustomMethod]
        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<TSource>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<TSource>, Task<TSource>>(FirstAsync<TSource>).Method
                    , source.Expression));
        }
        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
            => source?.Where(predicate).FirstAsync();



        [ExpressionNode_CustomMethod]
        [StreamQuery_CustomMethod]
        public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<TSource>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<TSource>, Task<TSource>>(LastOrDefaultAsync<TSource>).Method
                    , source.Expression));
        }
        public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
            => source?.Where(predicate).LastOrDefaultAsync();



        [ExpressionNode_CustomMethod]
        [StreamQuery_CustomMethod]
        public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<TSource>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<TSource>, Task<TSource>>(LastAsync<TSource>).Method
                    , source.Expression));
        }
        public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
            => source?.Where(predicate).LastAsync();




    }





}