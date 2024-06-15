using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Vitorm_Extensions;
using System.Data;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Type_String_Like_Test
    {


        [TestMethod]
        public void Test_Like()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            var users = userQuery.ToList();
            users.ForEach(row => row.name = "u|" + row.id + "|" + (row.fatherId.ToString() ?? "") + "|" + (row.motherId.ToString() ?? ""));
            dbContext.UpdateRange(users);

            // StartsWith
            {
                var query = userQuery.Where(u => u.name.StartsWith("u|3|5"));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
                Assert.AreEqual("u|3|5|6", userList.First().name);
            }
            // EndsWith
            {
                var query = userQuery.Where(u => u.name.EndsWith("3|5|6"));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
                Assert.AreEqual("u|3|5|6", userList.First().name);
            }
            // Contains
            {
                var query = userQuery.Where(u => u.name.Contains("|3|5|"));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
                Assert.AreEqual("u|3|5|6", userList.First().name);
            }
        }



    }
}
