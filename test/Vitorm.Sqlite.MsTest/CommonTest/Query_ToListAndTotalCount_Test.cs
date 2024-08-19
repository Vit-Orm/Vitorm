using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_ToListAndTotalCount_Test
    {

        [TestMethod]
        public void Test_ToListAndTotalCount()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            var query = userQuery
                     .Where(user => user.id > 2)
                     .OrderBy(m => m.id)
                     .Select(user => new { id = user.id, name = user.name })
                     ;


            Test(query, expectedCount: 4, expectedTotalCount: 4);


            Test(query.Skip(0), expectedCount: 4, expectedTotalCount: 4);
            Test(query.Skip(1), expectedCount: 3, expectedTotalCount: 4);
            Test(query.Skip(10), expectedCount: 0, expectedTotalCount: 4);

            Test(query.Take(0), expectedCount: 0, expectedTotalCount: 4);
            Test(query.Take(2), expectedCount: 2, expectedTotalCount: 4);
            Test(query.Take(20), expectedCount: 4, expectedTotalCount: 4);



            Test(query.Skip(0).Take(0), expectedCount: 0, expectedTotalCount: 4);
            Test(query.Skip(0).Take(2), expectedCount: 2, expectedTotalCount: 4);
            Test(query.Skip(0).Take(10), expectedCount: 4, expectedTotalCount: 4);

            Test(query.Skip(1).Take(0), expectedCount: 0, expectedTotalCount: 4);
            Test(query.Skip(1).Take(2), expectedCount: 2, expectedTotalCount: 4);
            Test(query.Skip(1).Take(10), expectedCount: 3, expectedTotalCount: 4);

            Test(query.Skip(10).Take(0), expectedCount: 0, expectedTotalCount: 4);
            Test(query.Skip(10).Take(2), expectedCount: 0, expectedTotalCount: 4);
            Test(query.Skip(10).Take(10), expectedCount: 0, expectedTotalCount: 4);

        }



        void Test<T>(IQueryable<T> query, int expectedCount, int expectedTotalCount)
        {
            // Count
            {
                var count = query.Count();
                Assert.AreEqual(expectedCount, count);
            }
            // TotalCount
            {
                var totalCount = query.TotalCount();
                Assert.AreEqual(expectedTotalCount, totalCount);
            }
            // ToListAndTotalCount
            {
                var (list, totalCount) = query.ToListAndTotalCount();
                Assert.AreEqual(expectedCount, list.Count);
                Assert.AreEqual(expectedTotalCount, totalCount);
            }
        }



    }
}
