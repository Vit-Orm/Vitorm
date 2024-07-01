using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

using Vit.Linq.ExpressionTree.ComponentModel;


namespace Vitorm
{
    public static class TypeUtil
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


        public static object DefaultValue(Type type)
        {
            if (null == type || !type.IsValueType) return null;
            return Activator.CreateInstance(type);
        }

        public static Model Clone<Model>(Model source)
        {
            if (null == source) return default;
            var type = source.GetType();
            var destination = (Model)Activator.CreateInstance(type);

            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (p.CanRead && p.CanWrite)
                {
                    var value = p.GetValue(source);
                    if (value == null) continue;
                    p.SetValue(destination, value);
                }
            }
            foreach (var p in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                {
                    var value = p.GetValue(source);
                    if (value == null) continue;
                    p.SetValue(destination, value);
                }
            }
            return destination;
        }

    }
}
