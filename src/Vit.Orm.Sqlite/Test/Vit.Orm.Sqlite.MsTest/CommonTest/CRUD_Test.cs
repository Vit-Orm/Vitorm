using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Linq_Extensions;
using System.Data;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class CRUD_Test
    {
        #region #1 Create

        [TestMethod]
        public void Test_Create()
        {
            var user = new User { id = 7, name = "testUser7", birth = DateTime.Now, fatherId = 1, motherId = 2 };
            var user2 = new User { id = 8, name = "testUser8", birth = DateTime.Now, fatherId = 3, motherId = 4 };

            // #1 Add
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                dbContext.Add(user);

                Assert.AreEqual(7, userQuery.Count());

                var newUser = userQuery.FirstOrDefault(m => m.id == 7);
                Assert.AreEqual(user.id, newUser?.id);
                Assert.AreEqual(user.name, newUser?.name);
            }

            // #2 AddRange
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                dbContext.AddRange(new[] { user, user2 });

                Assert.AreEqual(8, userQuery.Count());

                var newUsers = userQuery.Where(m => m.id >= 7).ToList();
                Assert.AreEqual(2, newUsers.Count());
                Assert.AreEqual(user.id, newUsers[0]?.id);
                Assert.AreEqual(user2.id, newUsers[1]?.id);
            }

        }
        #endregion


        #region #3 Update

        [TestMethod]
        public void Test_Update()
        {
            var birth = DateTime.Parse("2021-03-01 00:00:00");
            var user = new User { id = 4, name = "testUser4", birth = birth, fatherId = 14 };
            var user2 = new User { id = 5, name = "testUser5", birth = DateTime.Now, fatherId = 15 };

            #region Update
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = dbContext.Update(user);
                Assert.AreEqual(1, rowCount);

                var newUser = userQuery.FirstOrDefault(m => m.id == 4);
                Assert.AreEqual(4, newUser.id);
                Assert.AreEqual(user.name, newUser.name);
                Assert.AreEqual(user.birth, newUser.birth);
                Assert.AreEqual(user.fatherId, newUser.fatherId);
            }
            #endregion

            #region UpdateRange
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = dbContext.UpdateRange(new[] { user, user2 });
                Assert.AreEqual(2, rowCount);

                var newUsers = userQuery.Where(m => m.id == 4 || m.id == 5).ToList();
                Assert.AreEqual(user.id, newUsers[0].id);
                Assert.AreEqual(user.name, newUsers[0].name);
                Assert.AreEqual(user2.id, newUsers[1].id);
                Assert.AreEqual(user2.name, newUsers[1].name);
            }
            #endregion

        }


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
        #endregion


        #region #4 Delete


        [TestMethod]
        public void Test_Delete()
        {

            #region #1 Delete
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = dbContext.Delete(new User { id = 5 });

                Assert.AreEqual(1, rowCount);
                Assert.AreEqual(5, userQuery.Count());
            }
            #endregion

            #region #2 DeleteRange
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = dbContext.DeleteRange(new[] { new User { id = 5 }, new User { id = 6 }, new User { id = 10 } });

                Assert.AreEqual(2, rowCount);
                Assert.AreEqual(4, userQuery.Count());
            }
            #endregion

            #region #3 DeleteByKey
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = dbContext.DeleteByKey<User>(4);
                Assert.AreEqual(1, rowCount);
                Assert.AreEqual(5, userQuery.Count());
            }
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = dbContext.DeleteByKey<User>(7);
                Assert.AreEqual(0, rowCount);
                Assert.AreEqual(6, userQuery.Count());
            }
            #endregion


            #region #4 DeleteByKeys
            {
                using var dbContext = DataSource.CreateDbContext();
                var userSet = dbContext.DbSet<User>();

                var rowCount = userSet.DeleteByKeys(new[] { 5, 6, 10 });

                Assert.AreEqual(2, rowCount);
                Assert.AreEqual(4, userSet.Query().Count());
            }
            #endregion
        }



        [TestMethod]
        public void Test_ExecuteDelete()
        {
            if (1 == 1)
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var query = from user in userQuery
                            from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                            where user.id <= 5 && father != null
                            select new
                            {
                                user,
                                father
                            };

                var rowCount = query.ExecuteDelete();

                Assert.AreEqual(3, rowCount);

                var newUsers = userQuery.ToList();
                Assert.AreEqual(3, newUsers.Count());
                Assert.AreEqual(4, newUsers.First().id);
                Assert.AreEqual(6, newUsers.Last().id);
            }

            if (1 == 1)
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = userQuery.Where(m => m.id == 2 || m.id == 4).ExecuteDelete();

                Assert.AreEqual(2, rowCount);

                var newUsers = userQuery.ToList();
                Assert.AreEqual(4, newUsers.Count());
                Assert.AreEqual(1, newUsers.First().id);
                Assert.AreEqual(3, newUsers[1].id);
                Assert.AreEqual(5, newUsers[2].id);
            }
        }
        #endregion


    }
}
