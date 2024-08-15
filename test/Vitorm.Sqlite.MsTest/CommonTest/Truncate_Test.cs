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

            // Count
            {
                var count = dbContext.Query<User>().Count();
                Assert.AreEqual(6, count);
            }

            dbContext.Truncate<User>();

            DataSource.WaitForUpdate();

            // Count
            {
                var count = dbContext.Query<User>().Count();
                Assert.AreEqual(0, count);
            }
        }


    }
}
