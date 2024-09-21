using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public partial class Truncate_Test
    {

        [TestMethod]
        public void Test_Truncate()
        {
            using var dbContext = DataSource.CreateDbContextForWriting();

            // assert
            {
                var count = dbContext.Query<User>().Count();
                Assert.AreEqual(6, count);
            }

            dbContext.Truncate<User>();

            DataSource.WaitForUpdate();

            // assert
            {
                var count = dbContext.Query<User>().Count();
                Assert.AreEqual(0, count);
            }


            var user = User.NewUser(id: 1, forAdd: true);
            dbContext.Add(user);

            DataSource.WaitForUpdate();
            // assert
            {
                Assert.AreEqual(1, user.id);

                var list = dbContext.Query<User>().ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(1, list[0].id);
            }

        }


        [TestMethod]
        public async Task Test_TruncateAsync()
        {
            using var dbContext = DataSource.CreateDbContextForWriting();

            // assert
            {
                var count = dbContext.Query<User>().Count();
                Assert.AreEqual(6, count);
            }

            await dbContext.TruncateAsync<User>();

            DataSource.WaitForUpdate();

            // assert
            {
                var count = dbContext.Query<User>().Count();
                Assert.AreEqual(0, count);
            }
        }



    }
}
