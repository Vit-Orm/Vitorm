using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.Issue000_099.Issues
{
    /// <summary>
    /// https://github.com/Vit-Orm/Vitorm/issues/14
    /// #14 SQL Error [1067] [42000]: Invalid default value for 'name'
    /// </summary>
    [TestClass]
    public class Issue_014_Test
    {
        static void Test(string dataProviderName)
        {
            // #1 Init
            using var dbContext = Data.DataProvider(dataProviderName).CreateSqlDbContext();
            var dbSet = dbContext.DbSet<Setting>();
            dbSet.TryDropTable();
            dbSet.TryCreateTable();
            dbSet.Add(new Setting { name = "enable", value = "false" });

            // #2 Assert
            {
                var entity = dbSet.Query().Where(m => m.name == "enable").First();
                Assert.AreEqual("false", entity.value);
            }
        }


        [TestMethod]
        public void Test_SqlServer()
        {
            Test("SqlServer");
        }


        [TestMethod]
        public void Test_MySql()
        {
            Test("MySql");
        }


        [TestMethod]
        public void Test_Sqlite()
        {
            Test("Sqlite");
        }

        // Entity
        [Table("Issue014_Setting")]
        public class Setting
        {
            [System.ComponentModel.DataAnnotations.Key]
            public string name { get; set; }

            public string value { get; set; }
        }

    }


}

