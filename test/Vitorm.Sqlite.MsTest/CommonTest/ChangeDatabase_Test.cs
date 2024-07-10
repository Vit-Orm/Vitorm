using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public partial class ChangeDatabase_Test
    {

        [TestMethod]
        public void Test_ChangeDatabase()
        {
            string databaseName;

            // database
            {
                using var dbContext = DataSource.CreateDbContextForWriting(autoInit: false);

                databaseName = dbContext.databaseName;

                dbContext.Drop<User>();
                dbContext.Create<User>();

                var user = dbContext.Get<User>(1);
                Assert.IsNull(user);

                user = User.NewUser(id: 1, forAdd: true);
                user.name = "Hello database";
                dbContext.Add(user);
            }

            // database2
            {
                using var dbContext = DataSource.CreateDbContextForWriting(autoInit: false);

                dbContext.ChangeDatabase(databaseName + "2");

                dbContext.Drop<User>();
                dbContext.Create<User>();

                var user = dbContext.Get<User>(1);
                Assert.IsNull(user);

                user = User.NewUser(id: 1, forAdd: true);
                user.name = "Hello database2";
                dbContext.Add(user);
            }


            DataSource.WaitForUpdate();

            // database
            {
                using var dbContext = DataSource.CreateDbContextForWriting(autoInit: false);

                dbContext.ChangeDatabase(databaseName);
                var user = dbContext.Get<User>(1);
                Assert.AreEqual("Hello database", user.name);
            }

            // database2
            {
                using var dbContext = DataSource.CreateDbContextForWriting(autoInit: false);

                dbContext.ChangeDatabase(databaseName + "2");

                var user = dbContext.Get<User>(1);
                Assert.AreEqual("Hello database2", user.name);
            }


        }






    }
}
