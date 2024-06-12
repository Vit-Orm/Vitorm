using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Vitorm_Extensions;
using System.Data;


namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_InnerJoin_BySelectMany_Test
    {

        [TestMethod]
        public void Test_InnerJoin_Demo()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Linq Expresssion
            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id)
                    where user.id > 2
                    select new { user, father };

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().user.id);
                Assert.AreEqual(5, userList.First().father.id);
            }

            // Lambda Expression
            {
                var query =
                    userQuery.SelectMany(
                        user => userQuery.Where(father => user.fatherId == father.id)
                        , (user, father) => new { user, father }
                    )
                    .Where(row => row.user.id > 2)
                    .Select(row => new { row.user, row.father });

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().user.id);
                Assert.AreEqual(5, userList.First().father.id);
            }
        }


        [TestMethod]
        public void Test_InnerJoin_Complex()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Linq Expresssion
            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id)
                    from mother in userQuery.Where(mother => user.motherId == mother.id)
                    where user.id > 1
                    orderby father.id descending
                    select new
                    {
                        user,
                        father,
                        mother,
                        testId = user.id + 100,
                        hasFather = father != null ? true : false
                    };
                query = query.Skip(1).Take(1);

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(1, userList.Count);

                var first = userList.First();
                Assert.AreEqual(2, first.user.id);
                Assert.AreEqual(4, first.father.id);
                Assert.AreEqual(6, first.mother.id);
                Assert.AreEqual(102, first.testId);
                Assert.AreEqual(true, first.hasFather);
            }

            // Lambda Expression
            {
                var query =
                    userQuery.SelectMany(
                        user => userQuery.Where(father => user.fatherId == father.id)
                        , (user, father) => new { user, father }
                    ).SelectMany(
                        row => userQuery.Where(mother => row.user.motherId == mother.id)
                        , (row, mother) => new { row, mother }
                    )
                    .Where(row2 => row2.row.user.id > 1)
                    .OrderByDescending(row2 => row2.row.father.id)
                    .Select(row2 =>
                        new
                        {
                            row2.row.user,
                            row2.row.father,
                            row2.mother,
                            testId = row2.row.user.id + 100,
                            hasFather = row2.row.father != null ? true : false
                        }
                    );

                query = query.Skip(1).Take(1);

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(1, userList.Count);

                var first = userList.First();
                Assert.AreEqual(2, first.user.id);
                Assert.AreEqual(4, first.father.id);
                Assert.AreEqual(6, first.mother.id);
                Assert.AreEqual(102, first.testId);
                Assert.AreEqual(true, first.hasFather);
            }
        }





    }
}
