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

            using var dbContext = DataSource.CreateDbContextForWriting();
            var dbSet = dbContext.DbSet<ExpressionTester.User>();

            dbContext.Execute(sql: "DROP TABLE  if exists `User2`;");
            dbSet.Create();
            dbSet.AddRange(initUsers);

            var query = dbSet.Query();
            ExpressionTester.TestQueryable(query);
        }
    }
}
