using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Type_String_Caculate_Test
    {


        [TestMethod]
        public void Test_Caculate()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var userList = userQuery.Where(u => u.name + 1 == "u31").ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList[0].id);
            }
            {
                var userList = userQuery.Where(u => "u31" == u.name + 1).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList[0].id);
            }
            {
                var userList = userQuery.Where(u => u.name == "u" + 3).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList[0].id);
            }

        }


    }
}
