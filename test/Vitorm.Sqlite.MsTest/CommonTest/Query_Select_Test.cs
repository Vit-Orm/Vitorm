using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_Select_Test
    {
        class User2 : User
        {
            public User2() { }

            public User2(int nid) { id = nid; }
            public User2(string name) { this.name = name; }

            public User father;
        }


        [TestMethod]
        public void Test_Select()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select new User2(user.id)
                    ;

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count);

            }

            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select new User2(user.name)
                    ;

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count);
            }
            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select new User2(user.name)
                    {
                        id = user.id + 100,
                        fatherId = father.id,
                        father = father,
                    }
                    ;

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count);

            }

            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select new User2
                    {
                        id = user.id,
                        fatherId = father.id,
                    };

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count);

            }


            {
                var query =
                        userQuery
                        .Where(u => u.id > 1)
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .Where(userGroup => userGroup.Key.motherId != null)
                        .OrderByDescending(userGroup => userGroup.Key.fatherId)
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId,
                            rowCount = userGroup.Count(),
                            maxId = userGroup.Max(m => m.id)
                        })
                        .Skip(1)
                        .Take(1)
                        ;

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(4, userList[0].fatherId);
                Assert.AreEqual(6, userList[0].motherId);
                Assert.AreEqual(1, userList[0].rowCount);
                Assert.AreEqual(2, userList[0].maxId);

            }
            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select new
                    {
                        id = user.id,
                        user = user,
                        ext = new
                        {
                            father = father,
                            fid = father.id
                        }
                    };

                query = query.Skip(1).Take(2);

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(2, userList.Count);
            }

            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id == 1
                    orderby user.id
                    select user.id;

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
            }

            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userQuery.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select user;

                query = query.Skip(1).Take(2);

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(2, userList.Count);
            }


            // String.Format(format: "{0}_{1}_{2}", "0", "1", "2")
            {
                var query =
                    from user in userQuery
                    orderby user.id
                    select new
                    {
                        uniqueId1 = user.id + "_" + user.fatherId + "_" + user.motherId,
                        uniqueId2 = $"{user.id}_{user.fatherId}_{user.motherId}"
                    };
                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(6, userList.Count);
                Assert.AreEqual("1_4_6", userList[0].uniqueId1);
            }


        }




    }
}
