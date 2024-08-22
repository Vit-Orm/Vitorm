using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Property_String_Like_Test
    {

        [TestMethod]
        public void Test_Like()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // StartsWith
            {
                var query = userQuery.Where(u => u.name.StartsWith("u35"));
                //var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
                Assert.AreEqual("u356", userList.First().name);
            }
            // EndsWith
            {
                var query = userQuery.Where(u => u.name.EndsWith("356"));
                //var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
                Assert.AreEqual("u356", userList.First().name);
            }
            // Contains
            {
                var query = userQuery.Where(u => u.name.Contains("35"));
                //var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
                Assert.AreEqual("u356", userList.First().name);
            }
        }



    }
}
