using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Vit.Linq.ExpressionNodes.ExpressionConvertor.MethodCalls;

namespace Vitorm
{
    public static partial class Orm_Extensions
    {
        [ExpressionNode_CustomMethod]
        [StreamQuery_MethodConvertor_ExecuteUpdate]
        public static Task<int> ExecuteUpdateAsync<Entity, EntityToUpdate>(this IQueryable<Entity> source, Expression<Func<Entity, EntityToUpdate>> update)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<Task<int>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<Entity>, Expression<Func<Entity, EntityToUpdate>>, Task<int>>(ExecuteUpdateAsync).Method
                    , source.Expression
                    , update));
        }

    }



}