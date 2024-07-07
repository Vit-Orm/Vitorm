using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_Query_ScopeParam_Test_Test
    {

        [TestMethod]
        public void Test_Join_Demo()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // params form method arg
            QueryByArg(userQuery, 2);

            // params from scope
            {
                var id = 2;
                var query =
                        from user in userQuery
                        where user.id > id
                        orderby user.id
                        select new { user };

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(3, userList[0].user.id);
                Assert.AreEqual(4, userList[1].user.id);
            }

            // params from scope
            {
                var userArg = new { id = 2 };
                var query =
                        from user in userQuery
                        where user.id > userArg.id
                        orderby user.id
                        select new { user };

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(3, userList[0].user.id);
                Assert.AreEqual(4, userList[1].user.id);
            }

        }


        void QueryByArg(IQueryable<User> userQuery, int id)
        {
            // Linq Expression
            {
                var query =
                from user in userQuery
                where user.id > id
                orderby user.id
                select new { user };

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(3, userList[0].user.id);
                Assert.AreEqual(4, userList[1].user.id);
            }
        }




    }
}
