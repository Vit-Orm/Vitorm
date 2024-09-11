using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Distinct_Test
    {

        [TestMethod]
        public void Test_Distinct()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var query = userQuery.Select(u => u.fatherId).Distinct();

                var fatherIds = query.ToList();

                Assert.AreEqual(3, fatherIds.Count);
                Assert.AreEqual(0, fatherIds.Except(new int?[] { 4, 5, null }).Count());
            }
            {
                var query = userQuery.Select(u => new { u.fatherId }).Distinct();

                var userList = query.ToList();
                var fatherIds = userList.Select(u => u.fatherId).ToList();

                Assert.AreEqual(3, fatherIds.Count);
                Assert.AreEqual(0, fatherIds.Except(new int?[] { 4, 5, null }).Count());
            }
            {
                var query = userQuery.Select(u => new { u.fatherId, u.motherId }).Distinct();

                query = query.Skip(1).Take(100);

                var userList = query.ToList();
                var fatherIds = userList.Select(u => u.fatherId).ToList();
                var motherIds = userList.Select(u => u.motherId).ToList();

                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(0, fatherIds.Except(new int?[] { 4, 5, null }).Count());
                Assert.AreEqual(0, motherIds.Except(new int?[] { 6, null }).Count());
            }
            {
                var query = userQuery.Select(u => new { user = u, u.fatherId, u.motherId }).Distinct();

                var userList = query.ToList();

                Assert.AreEqual(6, userList.Count);
            }
            {
                var query = userQuery.Distinct();

                var userList = query.ToList();

                Assert.AreEqual(6, userList.Count);
            }

        }



    }
}
