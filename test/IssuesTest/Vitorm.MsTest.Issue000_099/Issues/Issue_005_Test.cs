using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.Issue000_099.Issues
{
    /// <summary>
    /// https://github.com/Vit-Orm/Vitorm/issues/5
    /// #5 single bool value in where condition
    /// </summary>
    [TestClass]
    public class Issue_005_Test
    {
        static void Test(string dataProviderName)
        {
            // #1 Init
            var name = Guid.NewGuid().ToString();
            using var dbContext = Data.DataProvider(dataProviderName).CreateSqlDbContext();
            var dbSet = dbContext.DbSet<MyUser>();
            dbSet.TryDropTable();
            dbSet.TryCreateTable();
            dbSet.Add(new MyUser { id = 1, enable = true, isEven = null });
            dbSet.Add(new MyUser { id = 2, enable = true, isEven = true });
            dbSet.Add(new MyUser { id = 3, enable = false, isEven = false });

            // #2 Assert
            {
                var user = dbSet.Query().Where(m => m.enable == true).OrderBy(m => m.id).First();
                Assert.AreEqual(1, user.id);
            }
            {
                var user = dbSet.Query().Where(m => m.enable == false).OrderBy(m => m.id).First();
                Assert.AreEqual(3, user.id);
            }
            {
                var user = dbSet.Query().Where(m => m.enable).OrderBy(m => m.id).First();
                Assert.AreEqual(1, user.id);
            }
            {
                var user = dbSet.Query().Where(m => !m.enable).OrderBy(m => m.id).First();
                Assert.AreEqual(3, user.id);
            }
            {
                var user = dbSet.Query().Where(m => m.isEven == null).OrderBy(m => m.id).First();
                Assert.AreEqual(1, user.id);
            }
            {
                var user = dbSet.Query().Where(m => m.isEven == true).OrderBy(m => m.id).First();
                Assert.AreEqual(2, user.id);
            }
            {
                var user = dbSet.Query().Where(m => m.isEven == false).OrderBy(m => m.id).First();
                Assert.AreEqual(3, user.id);
            }
            {
                var user = dbSet.Query().Where(m => m.isEven.Value).OrderBy(m => m.id).First();
                Assert.AreEqual(2, user.id);
            }
            {
                var user = dbSet.Query().Where(m => !m.isEven.Value).OrderBy(m => m.id).First();
                Assert.AreEqual(3, user.id);
            }
            {
                var query = dbSet.Query().Where(m => m.isEven.Value).OrderBy(m => m.isEven).Select(m => new { m.id, m.isEven });

                var users = query.ToList();
                var user = users.First();
                Assert.AreEqual(2, user.id);
                Assert.AreEqual(true, user.isEven);
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
        [Table("Issue005_MyUser")]
        public class MyUser
        {
            [Key]
            public int id { get; set; }
            public bool? isEven { get; set; }
            public bool enable { get; set; }
        }

    }


}

