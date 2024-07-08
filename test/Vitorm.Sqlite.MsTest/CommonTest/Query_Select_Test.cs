using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_Select_Test
    {

        [TestMethod]
        public void Test_Select()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // 
            {
                var userList = userQuery.ToList();
            }

            // userQuery.Select(user => user)
            {
                var rows = userQuery.Select(user => user).ToList();
                Assert.AreEqual(6, rows.Count);
            }

            // userQuery.Select(user => user.id);
            {
                var rows = userQuery.Select(user => user.id).ToList();
            }
            {
                var rows = userQuery.Select(user => user.id + 100).ToList();
            }
            {
                var rows = userQuery.Select(user => 1000 + user.id + 100).ToList();
            }
            {
                var rows = userQuery.Select(user => (double)user.id).ToList();
            }
            {
                var rows = userQuery.Select(user => (double)user.id + 0.1).ToList();
            }
            {
                var rows = userQuery.Select(user => user.id + 0.1).ToList();
            }

            {
                var query =
                    from user in userQuery
                    where user.id > 1
                    orderby user.id
                    select new
                    {
                        id = user.id,
                        user = user,
                        ext = new
                        {
                            father = user,
                            fid = user.fatherId
                        },
                        trueValue = true,
                    };

                var sql = query.ToExecuteString();
                var rows = query.ToList();
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
                Assert.AreEqual("1_4_6", userList[0].uniqueId2);
                Assert.AreEqual("4__", userList[3].uniqueId1);
                Assert.AreEqual("4__", userList[3].uniqueId2);
            }


        }




        [TestMethod]
        public void Test_Select_ExistClass()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var query =
                    from user in userQuery
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
                    where user.id > 1
                    orderby user.id
                    select new User2(user.name)
                    {
                        id = user.id + 100,
                        fatherId = user.fatherId,
                        father = user,
                    }
                    ;

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count);

            }

            {
                var query =
                    from user in userQuery
                    where user.id > 1
                    orderby user.id
                    select new User2
                    {
                        id = user.id,
                        fatherId = 12,
                    };

                var sql = query.ToExecuteString();
                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count);
            }
        }

        class User2 : User
        {
            public User2() { }
            public User2(int nid) { id = nid; }
            public User2(string name) { this.name = name; }
            public User father;
        }


    }
}
