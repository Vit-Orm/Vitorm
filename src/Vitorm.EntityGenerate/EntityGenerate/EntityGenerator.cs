#region << Version-v3 >>
/*
 * ========================================================================
 * Version： v3
 * Time   ： 2024-07-28
 * Author ： lith
 * Email  ： LithWang@outlook.com
 * Remarks： 
 * ========================================================================
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Vit.DynamicCompile.EntityGenerate
{
    /*
// https://www.cnblogs.com/sesametech-dotnet/p/13176329.html
// https://learn.microsoft.com/zh-cn/dotnet/api/system.reflection.emit.constructorbuilder?view=net-8.0

<ItemGroup>
    <PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
    <PackageReference Include="System.Reflection.Emit" Version="4.7.0" />
</ItemGroup>
     */

    internal class EntityGenerator
    {
        public static CustomAttributeBuilder GetAttributeBuilder<CustomAttribute>(
            IEnumerable<(Type type, object value)> constructorArgs = null
            , IEnumerable<(string name, object value)> propertyValues = null
            )
            where CustomAttribute : Attribute
        {
            constructorArgs ??= new List<(Type type, object value)>();
            propertyValues ??= new List<(string name, object value)>();

            var type = typeof(CustomAttribute);
            var ci = type.GetConstructor(constructorArgs.Select(m => m.type).ToArray());
            var builder = new CustomAttributeBuilder(
                ci
                , constructorArgs.Select(m => m.value).ToArray()
                , propertyValues.Select(property => type.GetProperty(property.name)).ToArray()
                , propertyValues.Select(m => m.value).ToArray()
                );
            return builder;
        }

        public static CustomAttributeBuilder GetAttributeBuilder<CustomAttribute>
            (IEnumerable<object> constructorArgs, IEnumerable<(string name, object value)> propertyValues = null)
            where CustomAttribute : Attribute
            => GetAttributeBuilder<CustomAttribute>(constructorArgs.Select(value => (value.GetType(), value)), propertyValues);


        public static Type CreateType(TypeDescriptor typeDescriptor)
        {
            // #1 define Type
            var assemblyName = new AssemblyName(typeDescriptor.assemblyName);
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule(typeDescriptor.moduleName);
            var typeBuilder = dynamicModule.DefineType(typeDescriptor.typeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);     // This is the type of class to derive from. Use null if there isn't one

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public |
                                                MethodAttributes.SpecialName |
                                                MethodAttributes.RTSpecialName);

            // #2 Add Property
            typeDescriptor.properties?.ForEach(property => AddProperty(typeBuilder, property));


            // #3 attributes
            typeDescriptor.attributes?.ForEach(attribute => typeBuilder.SetCustomAttribute(attribute));

            var generatedType = typeBuilder.CreateTypeInfo().AsType();
            return generatedType;
        }
        private static void AddProperty(TypeBuilder typeBuilder, PropertyDescriptor propertyDescriptor)
        {
            var propertyName = propertyDescriptor.name;
            var propertyType = propertyDescriptor.type;

            // #1 field
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // #2 get
            {
                var getMethod = typeBuilder.DefineMethod("get_" + propertyName,
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
                var getMethodIL = getMethod.GetILGenerator();
                getMethodIL.Emit(OpCodes.Ldarg_0);
                getMethodIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getMethodIL.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethod);
            }

            // #3 set
            {
                var setMethod = typeBuilder.DefineMethod("set_" + propertyName,
                      MethodAttributes.Public |
                      MethodAttributes.SpecialName |
                      MethodAttributes.HideBySig,
                      null, new[] { propertyType });
                var setMethodIL = setMethod.GetILGenerator();
                Label modifyProperty = setMethodIL.DefineLabel();
                Label exitSet = setMethodIL.DefineLabel();

                setMethodIL.MarkLabel(modifyProperty);
                setMethodIL.Emit(OpCodes.Ldarg_0);
                setMethodIL.Emit(OpCodes.Ldarg_1);
                setMethodIL.Emit(OpCodes.Stfld, fieldBuilder);
                setMethodIL.Emit(OpCodes.Nop);
                setMethodIL.MarkLabel(exitSet);
                setMethodIL.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setMethod);
            }

            // #4 attributes
            propertyDescriptor.attributes?.ForEach(attribute => propertyBuilder.SetCustomAttribute(attribute));
        }
    }




    #region TypeDescriptor
    public class TypeDescriptor
    {
        public string assemblyName;
        public string moduleName;
        /// <summary>
        /// The full path of the type. name cannot contain embedded nulls.
        /// </summary>
        public string typeName;

        public List<CustomAttributeBuilder> attributes;

        public List<PropertyDescriptor> properties;

        public TypeDescriptor() { }

        public TypeDescriptor(Dictionary<string, Type> propertiesMap)
        {
            assemblyName = Guid.NewGuid().ToString();
            moduleName = "Main";
            typeName = "DynamicClass";
            properties = propertiesMap.AsEnumerable().Select(kv => new PropertyDescriptor { name = kv.Key, type = kv.Value }).ToList();
        }

        public TypeDescriptor AddAttribute<CustomAttribute>(
           IEnumerable<(Type type, object value)> constructorArgs = null
           , IEnumerable<(string name, object value)> propertyValues = null
           )
           where CustomAttribute : Attribute
        {
            attributes ??= new();
            attributes.Add(EntityGenerator.GetAttributeBuilder<CustomAttribute>(constructorArgs, propertyValues));
            return this;
        }
        public TypeDescriptor AddAttribute<CustomAttribute>(IEnumerable<object> constructorArgs, IEnumerable<(string name, object value)> propertyValues = null)
           where CustomAttribute : Attribute
           => AddAttribute<CustomAttribute>(constructorArgs.Select(value => (value.GetType(), value)), propertyValues);


        public TypeDescriptor AddProperty(PropertyDescriptor property)
        {
            properties ??= new();
            properties.Add(property);
            return this;
        }
    }

    public class PropertyDescriptor
    {
        public string name;
        public Type type;
        public List<CustomAttributeBuilder> attributes;


        public PropertyDescriptor() { }
        public PropertyDescriptor(string name, Type type) { this.name = name; this.type = type; }

        public static PropertyDescriptor New<ValueType>(string name) => new PropertyDescriptor(name, typeof(ValueType));


        public PropertyDescriptor AddAttribute<CustomAttribute>(
            IEnumerable<(Type type, object value)> constructorArgs = null
            , IEnumerable<(string name, object value)> propertyValues = null
            )
            where CustomAttribute : Attribute
        {
            attributes ??= new();
            attributes.Add(EntityGenerator.GetAttributeBuilder<CustomAttribute>(constructorArgs, propertyValues));
            return this;
        }
        public PropertyDescriptor AddAttribute<CustomAttribute>(IEnumerable<object> constructorArgs, IEnumerable<(string name, object value)> propertyValues = null)
           where CustomAttribute : Attribute
           => AddAttribute<CustomAttribute>(constructorArgs.Select(value => (value.GetType(), value)), propertyValues);
    }
    #endregion

}
