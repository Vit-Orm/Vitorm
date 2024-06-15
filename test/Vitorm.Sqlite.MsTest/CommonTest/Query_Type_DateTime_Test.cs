using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Vitorm_Extensions;
using System.Data;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Type_DateTime_Test
    {

        [TestMethod]
        public void Test_Equal()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // ==
            {
                var userList = userQuery.Where(u => u.birth == new DateTime(2021, 01, 01, 03, 00, 00)).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
            }
        }



        [TestMethod]
        public void Test_Caculate()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var userList = userQuery.Where(u => u.birth == DateTime.Parse("2021-01-01 01:00:00").AddHours(2)).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
            }

        }



    }
}
