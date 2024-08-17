using System;
using System.Linq;
using System.Linq.Expressions;

using Vit.Linq.ExpressionNodes.ComponentModel;
using Vit.Linq.ExpressionNodes.ExpressionConvertor.MethodCalls;

using Vitorm.StreamQuery;
using Vitorm.StreamQuery.MethodCall;

namespace Vitorm
{
    public static partial class Orm_Extensions
    {
        [ExpressionNode_CustomMethod]
        [StreamQuery_MethodConvertor_ExecuteUpdate]
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

    /// <summary>
    /// Mark this method to be able to convert to IStream from ExpressionNode when executing query. For example : query.ToListAsync() 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class StreamQuery_MethodConvertor_ExecuteUpdateAttribute : Attribute, Vitorm.StreamQuery.MethodCall.IMethodConvertor
    {
        public IStream Convert(MethodCallConvertArgrument methodConvertArg)
        {
            ExpressionNode_MethodCall call = methodConvertArg.node;
            var reader = methodConvertArg.reader;
            var arg = methodConvertArg.arg;

            var source = reader.ReadStream(arg, call.arguments[0]);
            ExpressionNode_Lambda resultSelector = call.arguments[1];
            switch (source)
            {
                case SourceStream sourceStream:
                    {
                        var parameterName = resultSelector.parameterNames[0];
                        var parameterValue = ExpressionNode_RenameableMember.Member(stream: sourceStream, resultSelector.Lambda_GetParamTypes()[0]);

                        var select = reader.ReadResultSelector(arg.WithParameter(parameterName, parameterValue), resultSelector);
                        return new StreamToUpdate(sourceStream, call.methodName) { fieldsToUpdate = select.fields };
                    }
                case CombinedStream combinedStream:
                    {
                        var parameterName = resultSelector.parameterNames[0];
                        var parameterValue = combinedStream.select.fields;
                        var select = reader.ReadResultSelector(arg.WithParameter(parameterName, parameterValue), resultSelector);

                        return new StreamToUpdate(source, call.methodName) { fieldsToUpdate = select.fields };
                    }
            }

            return null;

        }
    }



}