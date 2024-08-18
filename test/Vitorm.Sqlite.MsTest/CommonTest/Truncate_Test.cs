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
