using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq;

namespace Vitorm.MsTest.CommonTest
{
    public abstract partial class UserTest<User>
    {

        public async Task Test_GetAsync()
        {
            #region Get
            {
                var user = await Data.GetAsync<User>(1);
                Assert.AreEqual(1, user?.id);
            }
            #endregion
        }

        public async Task Test_QueryAsync()
        {
            #region Query
            {
                var userList = await Data.Query<User>().Where(u => u.id == 1).ToListAsync();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(1, userList.First().id);
            }
            #endregion
        }

        public async Task Test_QueryJoinAsync()
        {
            #region Query
            {
                var query =
                    from user in Data.Query<User>()
                    from father in Data.Query<User>().Where(father => user.fatherId == father.id)
                    where user.id > 2
                    select new { user, father };

                var userList = await query.ToListAsync();
                Assert.AreEqual(1, userList.Count);
                Assert.AreEqual(3, userList.First().user.id);
                Assert.AreEqual(5, userList.First().father.id);

            }
            #endregion
        }

        public async Task Test_ExecuteUpdateAsync()
        {
            #region ExecuteUpdate
            {
                var query = Data.Query<User>();

                var count = await query.ExecuteUpdateAsync(row => new User
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
        public async Task Test_ExecuteDeleteAsync()
        {
            #region ExecuteDelete
            {
                var query = Data.Query<User>();

                var count = await query.Where(u => u.id == 6).ExecuteDeleteAsync();

                //Assert.AreEqual(1, count);
                WaitForUpdate();

                var userList = query.ToList();
                Assert.AreEqual(5, userList.Count());
            }
            #endregion
        }
        public async Task Test_CreateAsync()
        {
            #region Create :  Add AddRange
            {
                var newUserList = NewUsers(7, 4, forAdd: true);

                // #1 Add
                await Data.AddAsync<User>(newUserList[0]);

                // #2 AddRange
                await Data.AddRangeAsync<User>(newUserList.Skip(1));

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

        public async Task Test_UpdateAsync()
        {
            #region Update: Update UpdateRange
            {
                var ids = Data.Query<User>().OrderBy(u => u.id).Select(u => u.id).ToArray()[^2..];

                var newUserList = ids.Select(id => NewUser(id)).Append(NewUser(ids.Last() + 1)).ToList();

                // Update
                {
                    var rowCount = await Data.UpdateAsync(newUserList[0]);
                    Assert.AreEqual(1, rowCount);
                }

                // UpdateRange
                {
                    var rowCount = await Data.UpdateRangeAsync(newUserList.Skip(1));
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

        public async Task Test_DeleteAsync()
        {
            #region Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            {
                // #1 Delete
                {
                    var rowCount = await Data.DeleteAsync(NewUser(1));
                    //Assert.AreEqual(1, rowCount);
                }

                // #2 DeleteRange
                {
                    var rowCount = await Data.DeleteRangeAsync(NewUsers(2, 2));
                    //Assert.AreEqual(2, rowCount);
                }

                // #3 DeleteByKey
                {
                    using var dbContext = Data.DataProvider<User>()?.CreateDbContext();
                    var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
                    var key = entityDescriptor.key;

                    var user = NewUser(4);
                    var keyValue = key.GetValue(user);
                    var rowCount = await Data.DeleteByKeyAsync<User>(keyValue);
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
                    var rowCount = await Data.DeleteByKeysAsync<User, object>(keyValues);
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

        public async Task Test_TruncateAsync()
        {
            await Data.TruncateAsync<User>();
        }

    }
}
