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

        protected List<IValueReader> constructorArgsReader;
        protected List<(IValueReader valueReader, Action<object, object> SetValue)> membersReader;

        public ModelReader(EntityReaderConfig config, EntityReader entityReader, ExpressionNode_New newNode)
        {
            entityType = newNode.New_GetType();

            var constructorArgTypes = newNode.New_GetConstructorArgTypes();
            var argsCount = constructorArgTypes?.Length ?? 0;

            constructor = entityType.GetConstructors().Where(
                    (constructor) =>
                    {
                        var parameters = constructor.GetParameters();

                        if (parameters.Length != argsCount) return false;
                        for (var i = 0; i < argsCount; i++)
                        {
                            if (constructorArgTypes[i] != parameters[i].ParameterType) return false;
                        }
                        return true;
                    }
              ).First();

            constructorArgsReader = newNode.constructorArgs?.Select((arg, i) => entityReader.BuildValueReader(config, arg.value, constructorArgTypes[i])).ToList();

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

                return (entityReader.BuildValueReader(config, arg.value, valueType), SetValue);
            }).ToList();
        }

        public object Read(IDataReader reader)
        {
            // invoke constructor to create new Object
            var parameters = constructorArgsReader.Select(argReader => argReader.Read(reader)).ToArray();
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
