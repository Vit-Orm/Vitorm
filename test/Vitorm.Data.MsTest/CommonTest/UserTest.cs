using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Extensions.Vitorm_Extensions;

using Vitorm.DataProvider;


namespace Vitorm.MsTest
{
    public abstract class UserTest<User> where User : Vitorm.MsTest.User, new()
    {

        public abstract User NewUser(int id);

        public virtual List<User> NewUsers(int startId, int count = 1)
        {
            return Enumerable.Range(startId, count).Select(NewUser).ToList();
        }


        public void Test()
        {
            #region #1 Get
            {
                var user = Data.Get<User>(1);
                Assert.AreEqual(1, user?.id);
            }
            #endregion

            #region #2 Query
            {
                var query =
                    from user in Data.Query<User>()
                    from father in Data.Query<User>().Where(father => user.fatherId == father.id)
                    where user.id > 2
                    select new { user, father };

                var sql = query.ToExecuteString();
                var userList = query.ToList();

                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().user.id);
                Assert.AreEqual(5, userList.First().father.id);

            }
            #endregion

            #region #3 ExecuteUpdate
            {
                var query = Data.Query<User>();

                var count = query.ExecuteUpdate(row => new User
                {
                    name = "u_" + row.id + "_" + (row.fatherId.ToString() ?? "") + "_" + (row.motherId.ToString() ?? ""),
                    birth = DateTime.Parse("2021-01-11 00:00:00")
                });

                Assert.AreEqual(6, count);

                var userList = query.ToList();
                Assert.AreEqual("u_1_4_6", userList.First().name);
                Assert.AreEqual(DateTime.Parse("2021-01-11 00:00:00"), userList.First().birth);
                Assert.AreEqual("u_6__", userList.Last().name);
            }
            #endregion

            #region #4 ExecuteDelete
            {
                var query = Data.Query<User>();

                var count = query.Where(u => u.id == 6).ExecuteDelete();

                Assert.AreEqual(1, count);

                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count());
            }
            #endregion



            #region #5 Create :  Add AddRange
            {
                var newUserList = NewUsers(7, 4);

                // #1 Add
                Data.Add<User>(newUserList[0]);

                // #2 AddRange
                Data.AddRange<User>(newUserList.Skip(1));


                Thread.Sleep(1000);

                // assert
                {
                    var userList = Data.Query<User>().Where(user => user.id >= 7).ToList();
                    Assert.AreEqual(newUserList.Count, userList.Count());
                    Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                    Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
                }

                try
                {
                    Data.Add<User>(newUserList[0]);
                    Assert.Fail("should not be able to add same key twice");
                }
                catch (Exception ex) when (ex is not AssertFailedException)
                {
                }
            }
            #endregion


            #region #6 Update: Update UpdateRange
            {
                var ids = Data.Query<User>().OrderBy(u => u.id).Select(u => u.id).ToArray()[^2..];
            
                var newUserList = ids.Select(NewUser).Append(NewUser(ids.Last()+1)).ToList();

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

                Thread.Sleep(1000);

                // assert
                {
                    var userList = Data.Query<User>().Where(m => ids.Contains(m.id)).ToList();
                    Assert.AreEqual(2, userList.Count());
                    Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                    Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
                }

            }
            #endregion



            #region #7 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            {
                // #1 Delete
                {
                    var rowCount = Data.Delete(NewUser(1));
                    Assert.AreEqual(1, rowCount);
                }

                // #2 DeleteRange
                {
                    var rowCount = Data.DeleteRange(NewUsers(2, 2));
                    Assert.AreEqual(2, rowCount);
                }

                // #3 DeleteByKey
                {
                    using var dbContext = (Data.DataProvider<User>() as SqlDataProvider)?.CreateDbContext();
                    var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
                    var key = entityDescriptor.key;

                    var user = NewUser(4);
                    var keyValue = key.GetValue(user);
                    var rowCount = Data.DeleteByKey<User>(keyValue);
                    Assert.AreEqual(1, rowCount);
                }

                // #4 DeleteByKeys
                {
                    using var dbContext = (Data.DataProvider<User>() as SqlDataProvider)?.CreateDbContext();
                    var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
                    var key = entityDescriptor.key;

                    var users = Data.Query<User>().ToList();
                    var keyValues = users.Select(user => key.GetValue(user));
                    var rowCount = Data.DeleteByKeys<User, object>(keyValues);
                    Assert.AreEqual(users.Count, rowCount);
                }

                Thread.Sleep(1000);

                // assert
                {
                    var userList = Data.Query<User>().ToList();
                    Assert.AreEqual(0, userList.Count());
                }
            }
            #endregion



            #region #8 get DbContext and entityDescriptor
            {
                using var dbContext = (Data.DataProvider<User>() as SqlDataProvider)?.CreateDbContext();
                var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
                Assert.IsNotNull(entityDescriptor);
            }
            #endregion
        }
    }
}
