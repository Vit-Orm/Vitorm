using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Vitorm_Extensions;
using System.Data;


namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_LeftJoin_ByGroupJoin_Test
    {
        [TestMethod]
        public void Test_LeftJoin_Demo()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Linq Expression
            {
                var query =
                    from user in userQuery
                    join father in userQuery on user.fatherId equals father.id into fathers
                    from father in fathers.DefaultIfEmpty()
                    where user.id > 2
                    orderby user.id
                    select new { user, father };

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(3, userList[0].user.id);
                Assert.AreEqual(5, userList[0].father?.id);
                Assert.AreEqual(4, userList[1].user.id);
                Assert.AreEqual(null, userList[1].father?.name);
            }

            // Lambda Expression
            {
                var query =
                    userQuery.GroupJoin(
                        userQuery
                        , user => user.fatherId
                        , father => father.id
                        , (user, fathers) => new { user, fathers }
                    )
                    .SelectMany(
                        row => row.fathers.DefaultIfEmpty()
                        , (row, father) => new { row, father }
                    )
                    .Where(row2 => row2.row.user.id > 2)
                    .OrderBy(row2 => row2.row.user.id)
                    .Select(row2 => new { row2.row.user, row2.father });

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(4, userList.Count);
                Assert.AreEqual(3, userList[0].user.id);
                Assert.AreEqual(5, userList[0].father?.id);
                Assert.AreEqual(4, userList[1].user.id);
                Assert.AreEqual(null, userList[1].father?.name);
            }
        }


        [TestMethod]
        public void Test_LeftJoin_Complex()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Linq Expression
            {
                var query =
                    from user in userQuery
                    join father in userQuery on user.fatherId equals father.id into fathers
                    from father in fathers.DefaultIfEmpty()
                    join mother in userQuery on user.motherId equals mother.id into mothers
                    from mother in mothers.DefaultIfEmpty()
                    where user.id > 2
                    orderby user.id
                    select new
                    {
                        user,
                        father,
                        mother,
                        testId = user.id + 100,
                        hasFather = father.name != null ? true : false
                    };

                query = query.Skip(1).Take(2);

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(2, userList.Count);

                var first = userList.First();
                Assert.AreEqual(4, first.user.id);
                Assert.AreEqual(null, first.father?.name);
                Assert.AreEqual(null, first.mother?.name);
                Assert.AreEqual(104, first.testId);
                Assert.AreEqual(false, first.hasFather);
            }
        }


    }
}
