using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CustomTest
{

    [TestClass]
    public class DbFunction_Test
    {
        [TestMethod]
        public void Test_DbFunction()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();


            // select * from `User` as t0  where IIF(`t0`.`fatherId` is not null,true, false)
            {
                var query = userQuery.Where(u => DbFunction.Call<int>("IIF", u.fatherId != null, 1, 0) == 1);
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(3, userList.Count);
                Assert.AreEqual(3, userList.Last().id);
            }

            {
                var query = userQuery.Where(u => u.birth < DbFunction.Call<DateTime>("GETDATE"));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(6, userList.Count);
            }

            // coalesce(parameter1,parameter2, …)
            {
                var query = userQuery.Where(u => DbFunction.Call<int?>("coalesce", u.fatherId, u.motherId) != null);
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(3, userList.Count);
                Assert.AreEqual(1, userList.First().id);
            }


        }


    }
}
