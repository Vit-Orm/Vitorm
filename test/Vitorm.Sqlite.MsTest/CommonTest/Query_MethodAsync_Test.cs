using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_MethodAsync_Test
    {

        [TestMethod]
        public async Task Test_PlainQuery()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var userList = await userQuery.OrderBy(m => m.id).ToListAsync();
                Assert.AreEqual(6, userList.Count);
                Assert.AreEqual(1, userList.First().id);
                Assert.AreEqual(6, userList.Last().id);
            }

            {
                var userList = await userQuery.OrderBy(m => m.id).Select(u => u.id).ToListAsync();
                Assert.AreEqual(6, userList.Count);
                Assert.AreEqual(1, userList.First());
                Assert.AreEqual(6, userList.Last());
            }
        }


        [TestMethod]
        public async Task Test_Count()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Count
            {
                var query = userQuery.Where(user => user.id > 2);

                var count = await query.CountAsync();
                Assert.AreEqual(4, count);
            }

            // Skip Take Count
            {
                var query = userQuery.Where(user => user.id > 2);

                query = query.Skip(1).Take(10);

                var count = await query.CountAsync();
                Assert.AreEqual(3, count);
            }
        }


        [TestMethod]
        public async Task Test_TotalCount()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // TotalCountAsync
            {
                var query = userQuery.Where(user => user.id > 2);

                var count = await query.TotalCountAsync();
                Assert.AreEqual(4, count);
            }

            // Skip Take TotalCountAsync
            {
                var query = userQuery.Where(user => user.id > 2);

                query = query.Skip(1).Take(10);

                var count = await query.TotalCountAsync();
                Assert.AreEqual(4, count);
            }
        }


        [TestMethod]
        public async Task Test_ToListAndTotal()
        {
            using var dbContext = DataSource.CreateDbContext();

            // ToListAndTotalCount
            {
                var query = dbContext.Query<User>().Where(user => user.id > 2).Skip(1).Take(2);
                var (list, totalCount) = await query.ToListAndTotalCountAsync();
                Assert.AreEqual(2, list.Count);
                Assert.AreEqual(4, totalCount);
            }
        }



        [TestMethod]
        public async Task Test_FirstOrDefault()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var id = await userQuery.OrderBy(m => m.id).Select(u => u.id).FirstOrDefaultAsync();
                Assert.AreEqual(1, id);
            }

            {
                var user = await userQuery.OrderBy(m => m.id).FirstOrDefaultAsync();
                Assert.AreEqual(1, user?.id);
            }

            {
                var user = await userQuery.FirstOrDefaultAsync(user => user.id == 3);
                Assert.AreEqual(3, user?.id);
            }

            {
                var user = await userQuery.FirstOrDefaultAsync(user => user.id == 13);
                Assert.AreEqual(null, user?.id);
            }

            {
                var user = await userQuery.OrderByDescending(m => m.id).FirstOrDefaultAsync();
                Assert.AreEqual(6, user?.id);
            }

            {
                var user = await userQuery.OrderBy(m => m.id).Skip(1).Take(2).FirstOrDefaultAsync();
                Assert.AreEqual(2, user?.id);
            }
        }


        [TestMethod]
        public async Task Test_First()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var user = await userQuery.OrderBy(m => m.id).FirstAsync();
                Assert.AreEqual(1, user?.id);
            }

            {
                var user = await userQuery.FirstAsync(user => user.id == 3);
                Assert.AreEqual(3, user?.id);
            }

            {
                try
                {
                    var user = await userQuery.FirstAsync(user => user.id == 13);
                    Assert.Fail("IQueryable.First should throw Exception");
                }
                catch (Exception ex) when (ex is not AssertFailedException)
                {
                }
            }

            {
                var user = await userQuery.OrderBy(m => m.id).Skip(1).Take(2).FirstAsync();
                Assert.AreEqual(2, user?.id);
            }
        }



        [TestMethod]
        public async Task Test_LastOrDefault()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var id = await userQuery.OrderBy(m => m.id).Select(u => u.id).FirstOrDefaultAsync();
                Assert.AreEqual(1, id);
            }
            {
                var user = await userQuery.OrderBy(m => m.id).LastOrDefaultAsync();
                Assert.AreEqual(6, user?.id);
            }

            {
                var user = await userQuery.LastOrDefaultAsync(user => user.id == 3);
                Assert.AreEqual(3, user?.id);
            }

            {
                var user = await userQuery.LastOrDefaultAsync(user => user.id == 13);
                Assert.AreEqual(null, user?.id);
            }

            {
                var user = await userQuery.OrderByDescending(m => m.id).LastOrDefaultAsync();
                Assert.AreEqual(1, user?.id);
            }

            {
                var user = await userQuery.OrderBy(m => m.id).Skip(1).Take(2).LastOrDefaultAsync();
                Assert.AreEqual(3, user?.id);
            }
        }


        [TestMethod]
        public async Task Test_Last()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var user = await userQuery.OrderBy(m => m.id).LastAsync();
                Assert.AreEqual(6, user?.id);
            }

            {
                var user = await userQuery.LastAsync(user => user.id == 3);
                Assert.AreEqual(3, user?.id);
            }

            {
                try
                {
                    var user = await userQuery.LastAsync(user => user.id == 13);
                    Assert.Fail("IQueryable.Last should throw Exception");
                }
                catch (Exception ex) when (ex is not AssertFailedException)
                {
                }
            }

            {
                var user = await userQuery.OrderBy(m => m.id).Skip(1).Take(2).LastAsync();
                Assert.AreEqual(3, user?.id);
            }
        }




    }
}
