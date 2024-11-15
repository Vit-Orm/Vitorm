using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vitorm.Entity.PropertyType
{

    public class PropertyValueType : IPropertyValueType
    {
        public PropertyValueType(Type type)
        {
            this.type = type;
        }

        public ETypeMode mode => ETypeMode.value;

        public Type type { get; set; }
    }

    public class PropertyObjectType : IPropertyObjectType
    {
        public PropertyObjectType(Type type, IPropertyDescriptor[] properties = null)
        {
            this.type = type;
            this.properties = properties;
        }

        public ETypeMode mode => ETypeMode.@object;
        public Type type { get; set; }
        public IPropertyDescriptor[] properties { get; set; }
    }

    public class PropertyArrayType : IPropertyArrayType
    {
        public PropertyArrayType(Type type, IPropertyType elementPropertyType = null)
        {
            this.type = type;
            this.elementPropertyType = elementPropertyType;
        }

        public ETypeMode mode => ETypeMode.array;
        public Type type { get; set; }
        public IPropertyType elementPropertyType { get; set; }
        public virtual object CreateArray(IEnumerable elements)
        {
            return GetMethod_CreateArray(elementPropertyType.type).Invoke(this, new object[] { type, elements });
        }


        static MethodInfo method_CreateArray = null;
        static MethodInfo GetMethod_CreateArray(Type elementType)
        {
            method_CreateArray ??= new Func<Type, IEnumerable, object>(CreateArray<object>).Method.GetGenericMethodDefinition();

            return method_CreateArray.MakeGenericMethod(elementType);
        }

        public static object CreateArray<Element>(Type arrayType, IEnumerable elements)
        {
            var items = elements as IEnumerable<Element> ?? elements.Cast<Element>();

            if (arrayType.IsArray) return items.ToArray();

            var array = Activator.CreateInstance(arrayType);
            if (array is IList list)
            {
                foreach (var item in items) list.Add(item);
                return array;
            }

            if (array is ISet<Element> set)
            {
                foreach (var item in items) set.Add(item);
                return array;
            }
            throw new InvalidOperationException($"Can not convert to Array, element type not match. Array type: {arrayType.FullName} . Element type: {typeof(Element).FullName}");
        }
    }




}
