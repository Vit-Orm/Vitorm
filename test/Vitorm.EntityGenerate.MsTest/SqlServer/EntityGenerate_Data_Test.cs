using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.EntityGenerate;

namespace Vitorm.MsTest.SqlServer
{

    [TestClass]
    public partial class EntityGenerate_Data_Test
    {

        [TestMethod]
        public void Test()
        {
            // #1 init
            var entityNamespace = "Vitorm.MsTest.SqlServer";
            var dbContext = Data.DataProvider(entityNamespace).CreateSqlDbContext();
            dbContext.Execute(@"
if object_id(N'[dev_orm].[GeneratedUser]', N'U') is not null  DROP TABLE [dev_orm].[GeneratedUser];
CREATE TABLE [dev_orm].[GeneratedUser] ([id] int NOT NULL PRIMARY KEY, [name] varchar(1000) );
Insert into [dev_orm].[GeneratedUser] ([id],[name]) values(1,'u146');
");

            // #2 test
            var dbSet = DataEntity.GenerateDbSet(entityNamespace: entityNamespace, tableName: "GeneratedUser", schemaName: "dev_orm");
            var entityType = dbSet.entityDescriptor.entityType;

            // GetEntity
            dynamic user = dbSet.Get(1);
            string name = user.name;
            Assert.AreEqual("u146", name);

            BaseTest.TestDbSet(dbSet);

        }




    }
}
