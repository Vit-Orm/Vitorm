using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq.ExpressionTree.ExpressionTreeTest;

namespace Vitorm.MsTest.CustomTest
{
    [TestClass]
    public class Query_Test
    {
        [TestMethod]
        public void TestQueryable()
        {
            var initUsers = ExpressionTester.GetSourceData();

            using var dbContext = DataSource.CreateDbContext();
            var dbSet = dbContext.DbSet<ExpressionTester.User>();

            dbSet.Create();
            dbSet.AddRange(initUsers);

            var query = dbSet.Query();
            ExpressionTester.TestQueryable(query);
        }
    }
}
