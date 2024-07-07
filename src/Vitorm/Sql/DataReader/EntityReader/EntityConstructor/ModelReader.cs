using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.Sql.DataReader.EntityReader.EntityConstructor
{

    public class ModelReader : IValueReader
    {
        public ConstructorInfo constructor { get; }
        public Type entityType { get; }

        protected List<(string argName, IValueReader valueReader)> constructorArgsReader;
        protected List<(string argName, IValueReader valueReader, Action<object, object> SetValue)> membersReader;


        protected static Type TryGetDataType(ExpressionNode node)
        {
            return node?.nodeType switch
            {
                NodeType.New => node.New_GetType(),
                NodeType.Member => node.Member_GetType(),
                NodeType.MethodCall => node.MethodCall_GetReturnType(),
                _ => default,
            };
        }

        public ModelReader(EntityReaderConfig config, EntityReader entityReader, ExpressionNode_New newNode)
        {
            entityType = newNode.New_GetType();

            var constructorArgInfos = newNode.constructorArgs?.Select(m => new { m.name, type = TryGetDataType(m.value) }).ToArray();
            var argsCount = constructorArgInfos?.Length ?? 0;

            constructor = entityType.GetConstructors().Where(
                    (constructor) =>
                    {
                        var parameters = constructor.GetParameters();

                        if (parameters.Length != argsCount) return false;
                        for (var i = 0; i < argsCount; i++)
                        {
                            var constructorArg = constructorArgInfos[i];
                            var parameter = parameters[i];
                            if (constructorArg.type != null)
                            {
                                if (constructorArg.type != parameter.ParameterType) return false;
                                continue;
                            }

                            if (constructorArg.name != null && parameter.Name != null && constructorArg.name != parameter.Name) return false;
                        }
                        return true;
                    }
              ).First();

            var parameters = constructor.GetParameters();
            constructorArgsReader = newNode.constructorArgs?.Select((arg, i) => (arg.name, entityReader.BuildValueReader(config, arg.value, parameters[i].ParameterType))).ToList();

            PropertyInfo property; FieldInfo field;
            membersReader = newNode.memberArgs?.Select(arg =>
            {
                Action<object, object> SetValue = null;
                Type valueType = null;

                if ((property = entityType.GetProperty(arg.name)) != null)
                {
                    valueType = property.PropertyType;
                    SetValue = property.SetValue;
                }
                else if ((field = entityType.GetField(arg.name)) != null)
                {
                    valueType = field.FieldType;
                    SetValue = field.SetValue;
                }
                else
                    return default;

                return (arg.name, entityReader.BuildValueReader(config, arg.value, valueType), SetValue);
            }).ToList();
        }

        public object Read(IDataReader reader)
        {
            // invoke constructor to create new Object
            var parameters = constructorArgsReader.Select(arg => arg.valueReader.Read(reader)).ToArray();
            var obj = constructor.Invoke(parameters);

            // set members
            membersReader?.ForEach(member =>
            {
                var value = member.valueReader?.Read(reader);
                if (value != null)
                    member.SetValue(obj, value);
            });

            return obj;
        }
    }


}
