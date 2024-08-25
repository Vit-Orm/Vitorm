using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.Entity;
using Vitorm.Entity.Loader.DataAnnotations;
using Vitorm.MsTest.CommonTest;
using Vitorm.MsTest.Sqlite;

namespace Vitorm.MsTest.Sqlite
{
    [MyTable(table = "CustomUser2")]
    public class CustomUser
    {
        [MyKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}


namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class EntityLoader_CustomLoader_Test
    {

        [TestMethod]
        public void Test_EntityLoader()
        {
            {
                var name = Guid.NewGuid().ToString();
                Data.TryDropTable<CustomUser>();
                Data.TryCreateTable<CustomUser>();
                Data.Add(new CustomUser { id = 1, name = name });

                Assert.AreEqual(name, Data.Get<CustomUser>(1).name);
            }
        }
    }


    #region Custom EntityLoader Attribute
    public class MyTableAttribute : Attribute
    {
        public string table { get; set; }
    }
    public class MyKeyAttribute : Attribute { }


    public class CustomEntityLoader : IEntityLoader
    {
        public void CleanCache() { }
        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptor(Type entityType) => LoadFromType(entityType);
        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptorWithoutCache(Type entityType) => LoadFromType(entityType);

        public static bool GetTableName(Type entityType, out string tableName, out string schema)
        {
            tableName = entityType?.GetCustomAttribute<MyTableAttribute>()?.table;
            schema = null;
            return tableName != null;
        }

        public static (bool success, IEntityDescriptor EntityDescriptor) LoadFromType(Type entityType)
        {
            if (!GetTableName(entityType, out var tableName, out var schema)) return default;

            IColumnDescriptor[] allColumns = entityType?.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo =>
                {
                    // #1 isKey
                    bool isKey = propertyInfo?.GetCustomAttribute<MyKeyAttribute>() != null;

                    // #2 column name and type
                    string columnName = propertyInfo.Name;
                    string columnDbType = null;
                    int? columnOrder = null;

                    // #3 isIdentity
                    var isIdentity = false;

                    // #4 isNullable
                    bool isNullable;
                    {
                        var type = propertyInfo.PropertyType;
                        if (type == typeof(string)) isNullable = true;
                        else
                        {
                            isNullable = TypeUtil.IsNullable(type);
                        }
                    }

                    return new ColumnDescriptor(
                        propertyInfo, columnName: columnName,
                        isKey: isKey, isIdentity: isIdentity, isNullable: isNullable,
                        columnDbType: columnDbType,
                        columnOrder: columnOrder
                        );
                }).Where(column => column != null).ToArray();

            return (true, new EntityDescriptor(entityType, allColumns, tableName, schema));
        }
    }
    #endregion
}
