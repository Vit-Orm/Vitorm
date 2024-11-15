using System;
using System.Collections;

namespace Vitorm.Entity.PropertyType
{

    public interface IPropertyType
    {
        ETypeMode mode { get; }

        Type type { get; }
    }


    public interface IPropertyValueType : IPropertyType
    {
    }


    public interface IPropertyObjectType : IPropertyType
    {
        IPropertyDescriptor[] properties { get; }
    }


    public interface IPropertyArrayType : IPropertyType
    {
        IPropertyType elementPropertyType { get; }
        object CreateArray(IEnumerable elements);
    }


    public interface IPropertyDictionaryType : IPropertyType
    {
        Type keyType { get; }
        IPropertyType valueType { get; }
    }





}
