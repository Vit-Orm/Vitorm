using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Orm_Extensions_ExecuteUpdate_Test
    {

        [TestMethod]
        public void Test_ExecuteUpdate()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var count = userQuery.ExecuteUpdate(row => new User
                {
                    name = "u_" + row.id + "_" + (row.fatherId.ToString() ?? "") + "_" + (row.motherId.ToString() ?? ""),
                    birth = DateTime.Parse("2021-01-11 00:00:00")
                });

                Assert.AreEqual(6, count);

                var userList = userQuery.ToList();
                Assert.AreEqual("u_1_4_6", userList.First().name);
                Assert.AreEqual(DateTime.Parse("2021-01-11 00:00:00"), userList.First().birth);
                Assert.AreEqual("u_6__", userList.Last().name);
            }

            {
                var query = from user in userQuery
                            from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                            select new
                            {
                                user,
                                father,
                                user.motherId
                            };

                var count = query.ExecuteUpdate(row => new User
                {
                    name = "u2_" + row.user.id + "_" + (row.father.id.ToString() ?? "") + "_" + (row.motherId.ToString() ?? "")
                });
                Assert.AreEqual(6, count);


                var userList = userQuery.ToList();
                Assert.AreEqual("u2_1_4_6", userList.First().name);
                Assert.AreEqual("u2_6__", userList.Last().name);
            }

            {
                var query = from user in userQuery
                            from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                            where user.id <= 5 && father != null
                            select new
                            {
                                user,
                                father,
                                user.motherId
                            };

                var count = query.ExecuteUpdate(row => new User
                {
                    name = "u3_" + row.user.id + "_" + (row.father.id.ToString() ?? "") + "_" + (row.motherId.ToString() ?? "")
                });
                Assert.AreEqual(3, count);


                var userList = userQuery.ToList();
                Assert.AreEqual("u3_1_4_6", userList[0].name);
                Assert.AreEqual("u3_3_5_6", userList[2].name);
                Assert.AreEqual("u2_4__", userList[3].name);
            }
        }
    }
}
