using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.Sql;

using User = Vitorm.MsTest.Sqlite2.User;

namespace Vitorm.MsTest.Sqlite2
{
    public class User : Vitorm.MsTest.CommonTest.UserBase
    {
    }
}


namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public partial class SqliteReadOnly_Test
    {

        [TestMethod]
        public void Test()
        {
            Init("SqliteReadOnly_Test.db");
            Init("SqliteReadOnly_Test.readonly.db");

            // #1 get from ReadOnly node
            {
                var user = Data.Get<User>(1);
                Assert.AreEqual("SqliteReadOnly_Test.readonly.db", user.name);
            }

            // #2 get from ReadWrite node by transaction
            {
                using var dbContext = Data.DataProvider<User>().CreateSqlDbContext();
                using var tran = dbContext.BeginTransaction();

                var user = dbContext.Get<User>(1);
                Assert.AreEqual("SqliteReadOnly_Test.db", user.name);
            }

            // #3 update and check
            {
                // get from ReadOnly node
                var user = Data.Get<User>(1);
                Assert.AreEqual("SqliteReadOnly_Test.readonly.db", user.name);

                // update to ReadWrite node
                user.name = "NewName";
                Data.Update(user);

                // get from ReadOnly node
                user = Data.Get<User>(1);
                Assert.AreEqual("SqliteReadOnly_Test.readonly.db", user.name);

                // get from ReadWrite node
                using var dbContext = Data.DataProvider<User>().CreateSqlDbContext();
                using var tran = dbContext.BeginTransaction();
                user = dbContext.Get<User>(1);
                Assert.AreEqual("NewName", user.name);
            }
        }


        public void Init(string fileName)
        {
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, fileName);
            using var dbContext = new SqlDbContext().UseSqlite(connectionString: $"data source={fileName};");

            dbContext.TryDropTable<User>();
            dbContext.TryCreateTable<User>();
            dbContext.Add(new User { id = 1, name = fileName });
        }


    }
}
