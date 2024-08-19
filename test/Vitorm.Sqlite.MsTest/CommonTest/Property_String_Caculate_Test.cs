using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Property_String_Caculate_Test
    {


        [TestMethod]
        public void Test_Caculate()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var userList = userQuery.Where(u => u.name + 1 == "u3561").ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList[0].id);
            }
            {
                var userList = userQuery.Where(u => "u3561" == u.name + 1).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList[0].id);
            }
            {
                var userList = userQuery.Where(u => u.name == "u35" + 6).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList[0].id);
            }

        }


    }
}
