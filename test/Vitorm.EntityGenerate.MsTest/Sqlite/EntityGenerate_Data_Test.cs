using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.EntityGenerate;


namespace Vitorm.MsTest.Sqlite
{

    [TestClass]
    public partial class EntityGenerate_Data_Test
    {

        [TestMethod]
        public void Test()
        {
            // #1 init
            var entityNamespace = "Vitorm.MsTest.Sqlite";
            var dbContext = Data.DataProvider(entityNamespace).CreateSqlDbContext();
            dbContext.Execute(@"
DROP TABLE if exists GeneratedUser;
CREATE TABLE GeneratedUser(id integer NOT NULL PRIMARY KEY,  name text DEFAULT NULL);
Insert into GeneratedUser(id,name) values(1,'u146');
");

            // #2 test
            var dbSet = DataEntity.GenerateDbSet(entityNamespace: entityNamespace, tableName: "GeneratedUser");
            var entityType = dbSet.entityDescriptor.entityType;

            // Get Entity
            dynamic user = dbSet.Get(1);
            string name = user.name;
            Assert.AreEqual("u146", name);

            BaseTest.TestDbSet(dbSet);


        }




    }
}
