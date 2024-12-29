using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_Group_FirstOrDefault_Test
    {

        [TestMethod]
        public void Test_Group_FirstOrDefault()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Linq Expression
            {
                var query =
                        from user in userQuery
                        where user.fatherId != null
                        group user by new { user.fatherId, user.motherId } into userGroup
                        orderby userGroup.Key.fatherId, userGroup.Key.motherId
                        select new { userGroup.Key.fatherId, userGroup.Key.motherId };

                var row = query.FirstOrDefault();

                Assert.AreEqual(4, row?.fatherId);
                Assert.AreEqual(6, row?.motherId);
            }

            // Lambda Expression
            {
                var query =
                        userQuery.Where(m => m.fatherId != null)
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .OrderBy(userGroup => userGroup.Key.fatherId).ThenBy(userGroup => userGroup.Key.motherId)
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId
                        })
                        ;

                var row = query.FirstOrDefault();

                Assert.AreEqual(4, row?.fatherId);
                Assert.AreEqual(6, row?.motherId);
            }
        }



    }
}
