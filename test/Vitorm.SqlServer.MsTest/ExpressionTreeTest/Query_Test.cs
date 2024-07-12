using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq.ExpressionTree.ExpressionTreeTest;

namespace Vitorm.MsTest.ExpressionTreeTest
{
    [TestClass]
    public class Query_Test
    {
        [TestMethod]
        public void TestQueryable()
        {
            var initUsers = ExpressionTester.GetSourceData();
            initUsers.ForEach(u => u.id = 0);

            using var dbContext = DataSource.CreateDbContextForWriting();
            var dbSet = dbContext.DbSet<ExpressionTester.User>();

            dbContext.Execute(sql: "IF OBJECT_ID(N'User2', N'U') IS  NOT  NULL \r\nDROP TABLE [User2];");
            dbSet.TryCreateTable();
            dbSet.AddRange(initUsers);

            var query = dbSet.Query();
            ExpressionTester.TestQueryable(query);
        }
    }
}
