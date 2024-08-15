using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public partial class Schema_Test
    {

        [TestMethod]
        public void Test_CreateAndTropTable()
        {
            using var dbContext = DataSource.CreateDbContextForWriting();

            dbContext.TryCreateTable<User>();
            dbContext.TryCreateTable<User>();
            dbContext.TryDropTable<User>();
            dbContext.TryDropTable<User>();
        }



    }
}
