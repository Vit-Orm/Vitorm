using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.Entity;
using Vitorm.Entity.Loader.DataAnnotations;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class EntityLoader_CustomLoador_Test
    {
        [TestMethod]
        public void Test_EntityDescriptor()
        {
            using var dbContext = DataSource.CreateDbContext();
            var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
            var key = entityDescriptor.key;

            Assert.AreEqual("userId", key.columnName);
        }


        [TestMethod]
        public void Test_EntityLoader()
        {
            using var dbContext = DataSource.CreateDbContext();

            // #1 EntityLoaderAttribute
            {
                var users = dbContext.Query<CustomUser2>().Where(m => m.name == "u146").ToList();
                var sql = dbContext.Query<CustomUser2>().Where(m => m.name == "u146").ToExecuteString();
                Assert.AreEqual(1, users.Count());
                Assert.AreEqual(1, users[0].id);
            }

            // #2 defaultEntityLoader
            {
                DbContext.defaultEntityLoader.loaders.Insert(0, new CustomEntityLoaderAttribute());

                var users = dbContext.Query<CustomUser>().Where(m => m.name == "u146").ToList();
                Assert.AreEqual(1, users.Count());
                Assert.AreEqual(1, users[0].id);
            }

        }


        #region Custom Entity

        [CustomEntityLoader]
        public class CustomUser2 : CustomUser
        {
        }


        [Property(name = "TableName", value = "User")]
        [Property(name = "Schema", value = "dbo")]
        public class CustomUser
        {
            [Label("Key")]
            [Label("Identity")]
            [Property(name = "ColumnName", value = "userId")]
            public int id { get; set; }

            [Property(name = "ColumnName", value = "userName")]
            [Property(name = "TypeName", value = "varchar(1000)")]
            [Label("Required")]
            public string name { get; set; }

            [Property(name = "ColumnName", value = "userBirth")]
            public DateTime? birth { get; set; }

            [Property(name = "ColumnName", value = "userFatherId")]
            public int? fatherId { get; set; }
            [Property(name = "ColumnName", value = "userMotherId")]
            public int? motherId { get; set; }

            [Label("NotMapped")]
            public string test { get; set; }

            public static CustomUser NewUser(int id, bool forAdd = false) => new CustomUser { id = id, name = "testUser" + id };
        }

        #endregion


        #region Custom EntityLoader Attribute

        [System.AttributeUsage(System.AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        public class LabelAttribute : Attribute
        {
            public LabelAttribute(string label) { this.label = label; }
            public string label { get; init; }
        }

        [System.AttributeUsage(System.AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        public class PropertyAttribute : Attribute
        {
            public string name { get; set; }
            public string value { get; set; }
        }
        #endregion

        #region Custom EntityLoader
        public class CustomEntityLoaderAttribute : Attribute, IEntityLoader
        {
            public void CleanCache() { }
            public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptor(Type entityType) => LoadFromType(entityType);
            public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptorWithoutCache(Type entityType) => LoadFromType(entityType);

            public static bool GetTableName(Type entityType, out string tableName, out string schema)
            {
                var properties = entityType?.GetCustomAttributes<PropertyAttribute>();
                tableName = properties?.FirstOrDefault(attr => attr.name == "TableName")?.value;
                schema = properties?.FirstOrDefault(attr => attr.name == "Schema")?.value;
                return tableName != null;
            }

            public static (bool success, IEntityDescriptor EntityDescriptor) LoadFromType(Type entityType)
            {
                if (!GetTableName(entityType, out var tableName, out var schema)) return default;

                IColumnDescriptor[] allColumns = entityType?.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(propertyInfo =>
                    {
                        var labels = propertyInfo?.GetCustomAttributes<LabelAttribute>();
                        var properties = propertyInfo?.GetCustomAttributes<PropertyAttribute>();

                        if (labels.Any(m => m.label == "NotMapped")) return null;

                        // #1 isKey
                        bool isKey = labels.Any(m => m.label == "Key");

                        // #2 column name and type
                        var columnName = properties.FirstOrDefault(attr => attr.name == "ColumnName")?.value ?? propertyInfo.Name;
                        var columnDbType = properties.FirstOrDefault(attr => attr.name == "TypeName")?.value;
                        int? columnOrder = int.TryParse(properties.FirstOrDefault(attr => attr.name == "ColumnOrder")?.value, out var order) ? order : null;

                        // #3 isIdentity
                        var isIdentity = labels.Any(m => m.label == "Identity");

                        // #4 isNullable
                        bool isNullable;
                        if (labels.Any(m => m.label == "Required")) isNullable = false;
                        else
                        {
                            var type = propertyInfo.PropertyType;
                            if (type == typeof(string)) isNullable = true;
                            else
                            {
                                isNullable = (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition());
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
}
