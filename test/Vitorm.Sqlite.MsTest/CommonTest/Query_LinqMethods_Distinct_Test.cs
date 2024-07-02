using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_LinqMethods_Distinct_Test
    {

        [TestMethod]
        public void Test_Distinct()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var query = userQuery.Select(u => new { u.fatherId }).Distinct();

                //var sql = query.ToExecuteString();
                var userList = query.ToList();
                var ids = userList.Select(u => u.fatherId).ToList();

                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int?[] { 4, 5, null }).Count());
            }
            {
                var query = userQuery.Select(u => u.fatherId).Distinct();

                //var sql = query.ToExecuteString();
                var ids = query.ToList();

                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int?[] { 4, 5, null }).Count());
            }
            {
                var query = userQuery.Distinct();

                //var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(6, userList.Count);
            }

        }



    }
}
