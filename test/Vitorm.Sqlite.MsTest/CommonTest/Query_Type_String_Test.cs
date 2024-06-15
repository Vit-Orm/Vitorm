using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data;
using System.Linq;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Type_String_Test
    {

        // Enumerable.Contains
        // Queryable.Contains
        [TestMethod]
        public void Test_In()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Enumerable.Contains
            {
                var userList = userQuery.Where(u => new[] { "u3", "u5" }.Contains(u.name)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u3", "u5" }).Count());
            }

            // Enumerable.Contains
            {
                var ids = new[] { "u3", "u5" }.AsEnumerable();
                var userList = userQuery.Where(u => ids.Contains(u.name)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u3", "u5" }).Count());
            }

            // Queryable.Contains
            {
                var ids = new[] { "u3", "u5" }.AsQueryable();
                var userList = userQuery.Where(u => ids.Contains(u.name)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u3", "u5" }).Count());
            }


            // not Contains
            {
                var userList = userQuery.Where(u => !new[] { "u3", "u5" }.Contains(u.name)).ToList();
                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u1", "u2", "u4", "u6" }).Count());
            }
        }


        [TestMethod]
        public void Test_Equal()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // #1 ==
            {
                var userList = userQuery.Where(u => u.name == "u3" || "u5" == u.name).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u3", "u5" }).Count());
            }

            // #2 !=
            {
                var userList = userQuery.Where(u => u.name != "u1").ToList();
                Assert.AreEqual(5, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u2", "u3", "u4", "u5", "u6" }).Count());
            }
            {
                var userList = userQuery.Where(u => "u1" != u.name).ToList();
                Assert.AreEqual(5, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u2", "u3", "u4", "u5", "u6" }).Count());
            }
        }










    }
}
