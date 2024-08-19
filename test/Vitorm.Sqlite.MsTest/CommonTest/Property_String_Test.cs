using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Property_String_Test
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
                var userList = userQuery.Where(u => new[] { "u356", "u500" }.Contains(u.name)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u356", "u500" }).Count());
            }

            // Enumerable.Contains
            {
                var ids = new[] { "u356", "u500" }.AsEnumerable();
                var userList = userQuery.Where(u => ids.Contains(u.name)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u356", "u500" }).Count());
            }

            // Queryable.Contains
            {
                var ids = new[] { "u356", "u500" }.AsQueryable();
                var userList = userQuery.Where(u => ids.Contains(u.name)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u356", "u500" }).Count());
            }


            // not Contains
            {
                var userList = userQuery.Where(u => !new[] { "u356", "u500" }.Contains(u.name)).ToList();
                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u146", "u246", "u400", "u600" }).Count());
            }
        }


        [TestMethod]
        public void Test_Equal()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // #1 ==
            {
                var userList = userQuery.Where(u => u.name == "u356" || "u500" == u.name).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u356", "u500" }).Count());
            }

            // #2 !=
            {
                var userList = userQuery.Where(u => u.name != "u146").ToList();
                Assert.AreEqual(5, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u246", "u356", "u400", "u500", "u600" }).Count());
            }
            {
                var userList = userQuery.Where(u => "u146" != u.name).ToList();
                Assert.AreEqual(5, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.name).Except(new[] { "u246", "u356", "u400", "u500", "u600" }).Count());
            }
        }






    }
}
