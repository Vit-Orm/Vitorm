using System;
using System.Linq;
using System.Linq.Expressions;

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
        public static int ExecuteDelete(this IQueryable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    new Func<IQueryable, int>(ExecuteDelete).Method
                    , source.Expression));
        }
    }
}