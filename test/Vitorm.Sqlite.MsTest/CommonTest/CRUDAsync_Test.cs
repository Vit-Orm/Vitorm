using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public partial class CRUDAsync_Test
    {
        static DbContext CreateDbContext() => DataSource.CreateDbContextForWriting();

        #region #0 Schema
        [TestMethod]
        public async Task Test_Schema()
        {
            using var dbContext = CreateDbContext();

            await dbContext.TryDropTableAsync<User>();
            await dbContext.TryDropTableAsync<User>();

            await dbContext.TryCreateTableAsync<User>();
            await dbContext.TryCreateTableAsync<User>();
        }
        #endregion


        #region #1 Create
        [TestMethod]
        public async Task Test_Create()
        {
            using var dbContext = CreateDbContext();

            var newUserList = User.NewUsers(7, 4, forAdd: true);


            // #1 Add
            await dbContext.AddAsync(newUserList[0]);

            // #2 AddRange
            await dbContext.AddRangeAsync(newUserList.Skip(1));


            DataSource.WaitForUpdate();

            // assert
            {
                var userList = dbContext.Query<User>().Where(user => user.id >= 7).ToList();
                Assert.AreEqual(newUserList.Count, userList.Count());
                Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
            }

            try
            {
                await dbContext.AddAsync(newUserList[0]);
                Assert.Fail("should not be able to add same key twice");
            }
            catch (Exception ex) when (ex is not AssertFailedException)
            {
            }

        }
        #endregion

        #region #2 Retrieve : Get Query
        [TestMethod]
        public async Task Test_Retrieve()
        {
            using var dbContext = CreateDbContext();

            // #1 Get
            {
                var user = await dbContext.GetAsync<User>(1);
                Assert.AreEqual(1, user.id);
            }

            // #2 Query
            {
                var userList = await dbContext.Query<User>().ToListAsync();
                Assert.AreEqual(6, userList.Count());
            }
        }
        #endregion


        #region #3 Update
        [TestMethod]
        public async Task Test_Update()
        {
            using var dbContext = CreateDbContext();

            // Update
            {
                var rowCount = await dbContext.UpdateAsync(User.NewUser(4));
                Assert.AreEqual(1, rowCount);
            }

            // UpdateRange
            {
                var rowCount = await dbContext.UpdateRangeAsync(User.NewUsers(5, 3));
                Assert.AreEqual(2, rowCount);
            }

            DataSource.WaitForUpdate();

            // assert
            {
                var newUserList = User.NewUsers(4, 3, forAdd: false);
                var userList = dbContext.Query<User>().Where(m => m.id >= 4).ToList();
                Assert.AreEqual(newUserList.Count, userList.Count());
                Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
            }

        }
        #endregion


        #region #4 Delete
        [TestMethod]
        public async Task Test_Delete()
        {
            using var dbContext = CreateDbContext();

            // #1 Delete
            {
                var rowCount = await dbContext.DeleteAsync(User.NewUser(1));
                Assert.AreEqual(1, rowCount);
            }

            // #2 DeleteRange
            {
                var rowCount = await dbContext.DeleteRangeAsync(User.NewUsers(2, 2));
                Assert.AreEqual(2, rowCount);
            }

            // #3 DeleteByKey
            {
                var user = User.NewUser(4);
                var key = dbContext.GetEntityDescriptor(typeof(User)).key;
                var keyValue = key.GetValue(user);
                var rowCount = await dbContext.DeleteByKeyAsync<User>(keyValue);
                Assert.AreEqual(1, rowCount);
            }

            // #4 DeleteByKeys
            {
                var users = User.NewUsers(5, 2);
                var key = dbContext.GetEntityDescriptor(typeof(User)).key;
                var keyValues = users.Select(user => key.GetValue(user));
                var rowCount = await dbContext.DeleteByKeysAsync<User, object>(keyValues);
                Assert.AreEqual(2, rowCount);
            }

            DataSource.WaitForUpdate();

            // assert
            {
                var userList = dbContext.Query<User>().ToList();
                Assert.AreEqual(0, userList.Count());
            }
        }
        #endregion


    }
}
