using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Linq_Extensions;
using System.Data;


namespace Vit.Orm.Sqlite.MsTest
{
    [TestClass]
    public class Query_Group_Test
    {

        [TestMethod]
        public void Test_Group_Demo()
        {
            using var dbContext = DataSource.CreateFormatedDbContext(System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "_");
            var userQuery = dbContext.Query<User>();

            // Linq Expresssion
            {
                var query =
                        from user in userQuery
                        group user by new { user.fatherId, user.motherId } into userGroup
                        select new { userGroup.Key.fatherId, userGroup.Key.motherId  };
            
                var sql = query.ToExecuteString();
                var rows = query.ToList();

                Assert.AreEqual(3, rows.Count);
                Assert.AreEqual(4, rows[1].fatherId);
                Assert.AreEqual(6, rows[1].motherId);
                Assert.AreEqual(5, rows[2].fatherId);
                Assert.AreEqual(6, rows[2].motherId);
            }

            // Lambda Expression
            {
                var query =
                        userQuery
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId
                        })
                        ;

                var sql = query.ToExecuteString();
                var rows = query.ToList();

                Assert.AreEqual(3, rows.Count);
                Assert.AreEqual(4, rows[1].fatherId);
                Assert.AreEqual(6, rows[1].motherId);
                Assert.AreEqual(5, rows[2].fatherId);
                Assert.AreEqual(6, rows[2].motherId);
            }
        }


        [TestMethod]
        public void Test_Group_Complex()
        {
            using var dbContext = DataSource.CreateFormatedDbContext(System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "_");
            var userQuery = dbContext.Query<User>();

            // Linq Expresssion
            {
                var query =
                        from user in userQuery.Where(u => u.id > 1)
                        group user by new { user.fatherId, user.motherId } into userGroup
                        where userGroup.Key.motherId != null
                        orderby userGroup.Key.fatherId descending
                        select new { userGroup.Key.fatherId, userGroup.Key.motherId, rowCount = userGroup.Count(), maxId = userGroup.Max(m => m.id) };

                query = query.Skip(1).Take(1);

                var sql = query.ToExecuteString();
                var rows = query.ToList();

                Assert.AreEqual(1, rows.Count);
                Assert.AreEqual(4, rows[0].fatherId);
                Assert.AreEqual(6, rows[0].motherId);
                Assert.AreEqual(1, rows[0].rowCount);
                Assert.AreEqual(2, rows[0].maxId);
            }

            // Lambda Expression
            {
                var query =
                        userQuery
                        .Where(u => u.id > 1)
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .Where(userGroup => userGroup.Key.motherId != null)
                        .OrderByDescending(userGroup => userGroup.Key.fatherId)
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId,
                            rowCount = userGroup.Count(),
                            maxId = userGroup.Max(m => m.id)
                        })
                        .Skip(1)
                        .Take(1)
                        ;

                var sql = query.ToExecuteString();
                var rows = query.ToList();

                Assert.AreEqual(1, rows.Count);
                Assert.AreEqual(4, rows[0].fatherId);
                Assert.AreEqual(6, rows[0].motherId);
                Assert.AreEqual(1, rows[0].rowCount);
                Assert.AreEqual(2, rows[0].maxId);
            }
        }




        [TestMethod]
        public void Test_Others()
        {
            using var dbContext = DataSource.CreateFormatedDbContext(System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "_");
            var userQuery = dbContext.Query<User>();

            {
                var query =
                    userQuery
                    .GroupBy(user => new { user.fatherId, user.motherId })
                    .Select(userGroup => new
                    {
                        userGroup.Key.fatherId,
                        rowCount = userGroup.Count(),
                        maxId = userGroup.Max(m => m.id),
                        minId = userGroup.Min(m => m.id),
                        sumId = userGroup.Sum(m => m.id),
                        avgId = userGroup.Average(m => m.id)
                    })
                    ;

                var sql = query.ToExecuteString();
                var rows = query.ToList();

                Assert.AreEqual(3, rows.Count);
                Assert.AreEqual(2, rows[1].rowCount);
                Assert.AreEqual(1.5, rows[1].avgId);
            }
            {
                var query =
                    userQuery
                    .GroupBy(user => new { user.fatherId, user.motherId })
                    .Where(userGroup => userGroup.Key.motherId != null)
                    .OrderByDescending(userGroup => userGroup.Key.fatherId)
                    .Select(userGroup => new { userGroup.Key.fatherId, userGroup.Key.motherId })
                    ;

                var rows = query.ToList();
                var sql = query.ToExecuteString();

                Assert.AreEqual(2, rows.Count);
                Assert.AreEqual(5, rows[0].fatherId);
            }
            {
                var query =
                    userQuery
                    .GroupBy(user => user.fatherId)
                    .Where(userGroup => userGroup.Key != null)
                    .OrderByDescending(userGroup => userGroup.Key)
                    .Select(userGroup => new { fatherId = userGroup.Key, rowCount = userGroup.Count() })
                    ;

                var rows = query.ToList();
                var sql = query.ToExecuteString();

                Assert.AreEqual(2, rows.Count);
                Assert.AreEqual(5, rows[0].fatherId);
            }
        }


    }
}
