using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Property_Numric_Caculate_Test
    {

        [TestMethod]
        public void Test_Caculate()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var userList = userQuery.Where(u => u.id + 1 == 4).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
            }
            {
                var userList = userQuery.Where(u => 4 == u.id + 1).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
            }
            {
                var userList = userQuery.Where(u => u.id == 4 - 1).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
            }
        }


    }
}
