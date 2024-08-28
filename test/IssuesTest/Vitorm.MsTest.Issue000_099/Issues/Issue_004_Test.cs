using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.Issue000_099.Issues
{
    /// <summary>
    /// https://github.com/Vit-Orm/Vitorm/issues/4
    /// #4 support schema name of table for MySql
    /// </summary>
    [TestClass]
    public class Issue_004_Test
    {
        [TestMethod]
        public void Test_SqlServer()
        {
            var name = Guid.NewGuid().ToString();

            // #1 Init
            using var dbContext = Data.DataProvider("Vitorm.MsTest.SqlServer").CreateSqlDbContext();
            using var tran = dbContext.BeginTransaction();
            var dbSet = dbContext.DbSet<MyUser>();
            dbContext.Execute(@"create schema issue004_schema;");
            dbContext.Execute(@"
CREATE TABLE issue004_schema.Issue004_MyUser (id int NOT NULL primary key,  name varchar(1000) DEFAULT NULL);
insert into issue004_schema.Issue004_MyUser(id,name) values(1,@name);
", param: new Dictionary<string, object> { ["name"] = name });


            // #2 Assert
            {
                var user = dbSet.Get(1);
                Assert.AreEqual(name, user.name);
            }
        }


        [TestMethod]
        public void Test_MySql()
        {
            var name = Guid.NewGuid().ToString();

            // #1 Init
            using var dbContext = Data.DataProvider("Vitorm.MsTest.MySql").CreateSqlDbContext();
            using var tran = dbContext.BeginTransaction();
            var dbSet = dbContext.DbSet<MyUser>();
            dbContext.Execute(@"
create schema IF NOT EXISTS `issue004_schema`;
use `issue004_schema`;
drop table if exists `issue004_schema`.`Issue004_MyUser`;
CREATE TABLE IF NOT EXISTS `issue004_schema`.`Issue004_MyUser` (`id` int NOT NULL primary key,  `name` varchar(1000) DEFAULT NULL);
insert into `issue004_schema`.`Issue004_MyUser`(`id`,name) values(1,@name);
", param: new Dictionary<string, object> { ["name"] = name });

            // #2 Assert
            {
                var user = dbSet.Get(1);
                Assert.AreEqual(name, user.name);
            }
        }


        [TestMethod]
        public void Test_Sqlite()
        {
            var name = Guid.NewGuid().ToString();

            // #1 Init
            using var dbContext = Data.DataProvider("Vitorm.MsTest.Sqlite").CreateSqlDbContext();
            using var tran = dbContext.BeginTransaction();
            var dbSet = dbContext.DbSet<MyUser>();
            dbSet.TryDropTable();
            dbSet.TryCreateTable();
            dbSet.Add(new MyUser { id = 1, name = name });

            // #2 Assert
            {
                var user = dbSet.Get(1);
                Assert.AreEqual(name, user.name);
            }
        }

        // Entity
        [Table("Issue004_MyUser", Schema = "issue004_schema")]
        public class MyUser
        {
            [Key]
            public int id { get; set; }
            public string name { get; set; }
        }
    }
}

