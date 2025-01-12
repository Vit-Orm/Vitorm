using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq.ExpressionNodes.ExpressionNodesTest;

namespace Vitorm.MsTest.ExpressionNodesTest
{
    [TestClass]
    public class QueryAction_Test
    {
        [TestMethod]
        public void TestQueryable()
        {
            var initUsers = ExpressionTester.GetSourceData();

            using var dbContext = DataSource.CreateDbContextForWriting();
            var dbSet = dbContext.DbSet<ExpressionTester.User>();

            dbSet.TryDropTable();
            dbSet.TryCreateTable();
            dbSet.AddRange(initUsers);
            DataSource.WaitForUpdate();

            var query = dbSet.Query().OrderBy(m => m.id);
            ExpressionTester.TestQueryable(query);
        }
    }
}
