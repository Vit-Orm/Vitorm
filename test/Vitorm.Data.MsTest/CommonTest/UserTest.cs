using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Vitorm.MsTest
{
    public abstract partial class UserTest<User> where User : Vitorm.MsTest.UserBase, new()
    {
        public virtual void WaitForUpdate() { }
        public abstract User NewUser(int id, bool forAdd = false);

        public virtual List<User> NewUsers(int startId, int count = 1, bool forAdd = false)
        {
            return Enumerable.Range(startId, count).Select(id => NewUser(id, forAdd)).ToList();
        }


        public void Test_DbContext()
        {
            #region #0 get DbContext and entityDescriptor
            {
                using var dbContext = Data.DataProvider<User>()?.CreateDbContext();
                var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
                Assert.IsNotNull(entityDescriptor);
            }
            #endregion
        }

        public void Test_Transaction()
        {
            #region #0 Transaction
            {
                using var dbContext = Data.DataProvider<User>()?.CreateSqlDbContext();

                Assert.AreEqual("u400", dbContext.Get<User>(4).name);

                using (var tran1 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4001" });
                    Assert.AreEqual("u4001", dbContext.Get<User>(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u4002" });
                        Assert.AreEqual("u4002", dbContext.Get<User>(4).name);

                        var userSet = dbContext.DbSet<User>();
                        Assert.AreEqual("u4002", userSet.Get(4).name);
                    }
                    Assert.AreEqual("u4001", dbContext.Get<User>(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u4002" });
                        Assert.AreEqual("u4002", dbContext.Get<User>(4).name);
                        tran2.Rollback();
                    }
                    Assert.AreEqual("u4001", dbContext.Get<User>(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u4003" });
                        Assert.AreEqual("u4003", dbContext.Get<User>(4).name);
                        tran2.Commit();
                    }
                    Assert.AreEqual("u4003", dbContext.Get<User>(4).name);

                    //Assert.AreEqual("u400", Data.Get<User>(4).name);
                }

                Assert.AreEqual("u400", dbContext.Get<User>(4).name);
            }
            #endregion
        }



        public void Test_Get()
        {
            #region Get
            {
                var user = Data.Get<User>(1);
                Assert.AreEqual(1, user?.id);
            }
            #endregion
        }

        public void Test_Query()
        {
            #region Query
            {
                var userList = Data.Query<User>().Where(u => u.id == 1).ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(1, userList.First().id);
            }
            #endregion
        }

        public void Test_QueryJoin()
        {
            #region Query
            {
                var query =
                    from user in Data.Query<User>()
                    from father in Data.Query<User>().Where(father => user.fatherId == father.id)
                    where user.id > 2
                    select new { user, father };

                var userList = query.ToList();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().user.id);
                Assert.AreEqual(5, userList.First().father.id);

            }
            #endregion
        }
        public void Test_ToExecuteString()
        {
            #region ToExecuteString
            {
                var query = Data.Query<User>().Where(u => u.id == 1);

                var sql = query.ToExecuteString();
                Assert.IsNotNull(sql);
            }
            #endregion
        }
        public void Test_ExecuteUpdate()
        {
            #region ExecuteUpdate
            {
                var query = Data.Query<User>();

                var count = query.ExecuteUpdate(row => new User
                {
                    name = "u_" + row.id + "_" + (row.fatherId.ToString() ?? "") + "_" + (row.motherId.ToString() ?? ""),
                    birth = DateTime.Parse("2021-01-11 00:00:00")
                });

                Assert.AreEqual(6, count);

                WaitForUpdate();

                var userList = query.ToList();
                Assert.AreEqual("u_1_4_6", userList.First().name);
                Assert.AreEqual(DateTime.Parse("2021-01-11 00:00:00"), userList.First().birth);
                Assert.AreEqual("u_6__", userList.Last().name);
            }
            #endregion
        }
        public void Test_ExecuteDelete()
        {
            #region ExecuteDelete
            {
                var query = Data.Query<User>();

                var count = query.Where(u => u.id == 6).ExecuteDelete();

                //Assert.AreEqual(1, count);
                WaitForUpdate();

                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count());
            }
            #endregion
        }
        public void Test_Create()
        {
            #region Create :  Add AddRange
            {
                var newUserList = NewUsers(7, 4, forAdd: true);

                // #1 Add
                Data.Add<User>(newUserList[0]);

                // #2 AddRange
                Data.AddRange<User>(newUserList.Skip(1));

                WaitForUpdate();

                // assert
                {
                    var userList = Data.Query<User>().Where(user => user.id >= 7).ToList();
                    Assert.AreEqual(newUserList.Count, userList.Count());
                    Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                    Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
                }
            }
            #endregion
        }

        public void Test_Update()
        {
            #region Update: Update UpdateRange
            {
                var ids = Data.Query<User>().OrderBy(u => u.id).Select(u => u.id).ToArray()[^2..];

                var newUserList = ids.Select(id => NewUser(id)).Append(NewUser(ids.Last() + 1)).ToList();

                // Update
                {
                    var rowCount = Data.Update(newUserList[0]);
                    Assert.AreEqual(1, rowCount);
                }

                // UpdateRange
                {
                    var rowCount = Data.UpdateRange(newUserList.Skip(1));
                    Assert.AreEqual(1, rowCount);
                }

                WaitForUpdate();

                // assert
                {
                    var userList = Data.Query<User>().Where(m => ids.Contains(m.id)).ToList();
                    Assert.AreEqual(2, userList.Count());
                    Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                    Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
                }

            }
            #endregion
        }

        public void Test_Delete()
        {
            #region Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            {
                // #1 Delete
                {
                    var rowCount = Data.Delete(NewUser(1));
                    //Assert.AreEqual(1, rowCount);
                }

                // #2 DeleteRange
                {
                    var rowCount = Data.DeleteRange(NewUsers(2, 2));
                    //Assert.AreEqual(2, rowCount);
                }

                // #3 DeleteByKey
                {
                    using var dbContext = Data.DataProvider<User>()?.CreateDbContext();
                    var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
                    var key = entityDescriptor.key;

                    var user = NewUser(4);
                    var keyValue = key.GetValue(user);
                    var rowCount = Data.DeleteByKey<User>(keyValue);
                    //Assert.AreEqual(1, rowCount);
                }


                // assert
                {
                    WaitForUpdate();
                    var userList = Data.Query<User>().Where(u => u.id <= 4).ToList();
                    Assert.AreEqual(0, userList.Count());
                }


                // #4 DeleteByKeys
                {
                    using var dbContext = Data.DataProvider<User>()?.CreateDbContext();
                    var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
                    var key = entityDescriptor.key;

                    var users = Data.Query<User>().ToList();
                    var keyValues = users.Select(user => key.GetValue(user));
                    var rowCount = Data.DeleteByKeys<User, object>(keyValues);
                    //Assert.AreEqual(users.Count, rowCount);
                }


                // assert
                {
                    WaitForUpdate();
                    var userList = Data.Query<User>().ToList();
                    Assert.AreEqual(0, userList.Count());
                }
            }
            #endregion
        }

        public void Test_Truncate()
        {
            Data.Truncate<User>();
        }

    }
}
