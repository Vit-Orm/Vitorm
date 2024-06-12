using System;
using System.Linq;
using System.Linq.Expressions;

namespace Vit.Extensions.Vitorm_Extensions
{

    public static partial class Orm_Extensions
    {
        public static int ExecuteUpdate<Entity, EntityToUpdate>(this IQueryable<Entity> source, Expression<Func<Entity, EntityToUpdate>> update)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    new Func<IQueryable<Entity>, Expression<Func<Entity, EntityToUpdate>>, int>(ExecuteUpdate).Method
                    , source.Expression
                    , update));
        }
    }
}