using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_LeftJoin_BySelectMany_Test
    {




        [TestMethod]
        public void Test_LeftJoin_Demo()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();
 

            // Lambda Expression
            {
                var query =
                        userQuery.SelectMany(
                            user => userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                            , (user, father) => new { user, father }
                        )
                        .Where(row => row.user.id > 2)
                        .OrderBy(row => row.user.id)
                        .Select(row => new { row.user, row.father });

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
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    from userClass in dbContext.Query<UserClass>().Where(userClass => user.classId == userClass.id).DefaultIfEmpty()
                    where user.id > 1 && userClass.id == 1
                    orderby user.id
                    select new
                    {
                        user,
                        father,
                        mother,
                        userClass,
                        userClass.name,
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


        [TestMethod]
        public void Test_MultipleSelect()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();


            {
                var query = from user in userQuery
                            from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                            where user.id > 2 && father.name != null
                            orderby user.id
                            select new
                            {
                                user,
                                father
                            };

                var userList = query.ToList();

                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().user.id);
            }

            {
                var query = from user in userQuery
                            from father in userQuery.Where(father => user.fatherId == father.id)
                            from mother in userQuery.Where(mother => user.motherId == mother.id)
                            orderby user.id
                            select new
                            {
                                uniqueId = user.id + "_" + father.id + "_" + mother.id,
                                uniqueId1 = user.id + "_" + user.fatherId + "_" + user.motherId,
                                user,
                                user2 = user,
                                user3 = user,
                                father,
                                hasFather = user.fatherId != null ? true : false,
                                fatherName = father.name,
                                mother
                            };

                var userList = query.ToList();
                Assert.AreEqual(3, userList.Count);
                Assert.AreEqual(1, userList.First().user.id);
                Assert.AreEqual(3, userList.Last().user.id);
                Assert.AreEqual(5, userList.Last().father?.id);
            }

            {
                var query = from user in userQuery
                            from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                            from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                            orderby user.id
                            select new
                            {
                                user,
                                father,
                                userId = user.id + 100,
                                hasFather = user.fatherId != null ? true : false,
                                hasFather2 = father != null,
                                fatherName = father.name,
                                motherName = mother.name,
                            };

                var userList = query.ToList();

                Assert.AreEqual(6, userList.Count);
                Assert.AreEqual(1, userList.First().user.id);
                Assert.AreEqual(101, userList.First().userId);
                Assert.AreEqual(6, userList.Last().user.id);
                Assert.AreEqual(5, userList[2].father.id);
            }


        }



        [TestMethod]
        public void Test_Count()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var count = (from user in userQuery
                             from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                             where user.id > 2 && father.name == null
                             select new
                             {
                                 father
                             }).Count();

                Assert.AreEqual(3, count);
            }
        }


    }
}
