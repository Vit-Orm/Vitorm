using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Linq_Extensions;
using System.Data;

namespace Vitorm.MsTest
{

    [TestClass]
    public class DbFunction_Test
    {
        [TestMethod]
        public void Test_DbFunction()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();


            // select IF(500<1000,true,false)
            {
                var query = userQuery.Where(u => DbFunction.Call<bool>("IF", u.fatherId != null, true, false));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(3, userList.Count);
                Assert.AreEqual(3, userList.Last().id);
            }

            {
                var query = userQuery.Where(u => u.birth < DbFunction.Call<DateTime>("now"));
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
