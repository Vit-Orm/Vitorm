using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Vitorm_Extensions;
using System.Data;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Orm_Extensions_ToExecuteString_Test
    {

        [TestMethod]
        public void Test_ToExecuteString()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            #region users.Where().OrderBy().Skip().Take().ToList
            /*
            users.Where(row => row.user.id > 2)
            .OrderBy(user=>user.id)
            .Select(row => new {row.user })
            .Skip(1).Take(2);
             */
            {
                var query = (from user in userQuery
                             where user.id > 2 && !user.name.Contains("3")
                             orderby user.id descending
                             select new
                             {
                                 user
                             })
                            .Skip(1).Take(2);

                var sql = query.ToExecuteString();
                Assert.AreEqual(false, string.IsNullOrWhiteSpace(sql));

                var list = query.ToList();
                Assert.AreEqual(2, list.Count);
            }
            #endregion

        }
    }
}
