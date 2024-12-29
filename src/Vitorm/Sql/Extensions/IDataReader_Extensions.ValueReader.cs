using System;
using System.Data;

namespace Vitorm
{
    public static partial class IDataReader_Extensions
    {

        interface IValueReader
        {
            Type valueType { get; }
            object Read(IDataReader reader, int index = 0);
        }



        class ValueReader_String : IValueReader
        {
            public Type valueType => typeof(string);
            public object Read(IDataReader reader, int index = 0) => reader.GetString(index);


            public static ValueReader_String Instance = new ValueReader_String();
        }



        class ValueReader_Nullable : IValueReader
        {
            public ValueReader_Nullable(Type valueType)
            {
                this.valueType = valueType;
                underlyingType = TypeUtil.GetUnderlyingType(valueType);
            }
            public Type valueType { get; private set; }
            Type underlyingType;
            public object Read(IDataReader reader, int index = 0)
            {
                return TypeUtil.ConvertToUnderlyingType(reader[index], underlyingType);
            }
        }




        class ValueReader_Struct : IValueReader
        {
            public ValueReader_Struct(Type valueType)
            {
                this.valueType = valueType;
                defaultValue = TypeUtil.GetDefaultValue(valueType);
            }
            public Type valueType { get; private set; }
            object defaultValue;
            public object Read(IDataReader reader, int index = 0)
            {
                var value = reader[index];
                if (value == null || value == DBNull.Value) return defaultValue;
                return TypeUtil.ConvertToUnderlyingType(value, valueType);
            }
        }




    }
}
