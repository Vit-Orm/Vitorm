using System;
using System.Collections.Generic;
using System.Text;

namespace Vitorm.Sql
{
    public class TypeUtil
    {
        public static Type GetUnderlyingType(Type type)
        {
            if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        /// <summary>
        /// is ValueType or string or Nullable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueType(Type type)
        {
            if (type.IsValueType || type == typeof(string)) return true;
            if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
            {
                return true;
            }
            return false;
        }



        public static object ConvertToType(object value, Type type)
        {
            return ConvertToUnderlyingType(value, GetUnderlyingType(type));
        }

        public static object ConvertToUnderlyingType(object value, Type underlyingType)
        {
            if (value == null || value is DBNull) return null;

            if (!underlyingType.IsInstanceOfType(value))
                value = Convert.ChangeType(value, underlyingType);
            return value;
        }


    }
}
