using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data;
using System.Linq;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Type_Numeric_Test
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
                var userList = userQuery.Where(u => new[] { 3, 5 }.Contains(u.id)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 3,5 }).Count());
            }

            // Enumerable.Contains
            {
                var ids = new[] { 3, 5 }.AsEnumerable();
                var userList = userQuery.Where(u => ids.Contains(u.id)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 3, 5 }).Count());
            }

            // Queryable.Contains
            {
                var ids = new[] { 3, 5 }.AsQueryable();
                var userList = userQuery.Where(u => ids.Contains(u.id)).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 3, 5 }).Count());
            }


            // not Contains
            {
                var userList = userQuery.Where(u => !new[] { 3, 5 }.Contains(u.id)).ToList();
                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 1, 2, 4, 6 }).Count());
            }
        }


        [TestMethod]
        public void Test_Equal()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // #1 ==
            {
                var userList = userQuery.Where(u => u.id == 3 || 5 == u.id).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 3, 5 }).Count());
            }

            // #2 !=
            {
                var userList = userQuery.Where(u => u.id != 1).ToList();
                Assert.AreEqual(5, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 2, 3, 4, 5, 6 }).Count());
            }
            {
                var userList = userQuery.Where(u => 1 != u.id).ToList();
                Assert.AreEqual(5, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 2, 3, 4, 5, 6 }).Count());
            }
        }



        [TestMethod]
        public void Test_Compare()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // #1 > and <
            {
                var userList = userQuery.Where(u => u.id > 2).Where(m => m.id < 4).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().id);
            }

            // #2  > or <
            {
                var userList = userQuery.Where(u => u.id > 5 || u.id < 2).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 1, 6 }).Count());
            }

            // #3  >= or <=
            {
                var userList = userQuery.Where(u => u.id >= 6 || u.id <= 1).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 1, 6 }).Count());
            }

            // #4  in right side
            {
                var userList = userQuery.Where(u => 4 >= u.id && 3 <= u.id).ToList();
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(new[] { 3, 4 }).Count());
            }

        }





    }
}
