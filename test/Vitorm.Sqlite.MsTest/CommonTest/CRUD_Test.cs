using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public partial class CRUD_Test
    {
        static DbContext CreateDbContext() => DataSource.CreateDbContextForWriting();

        #region #0 Schema
        [TestMethod]
        public void Test_Schema()
        {
            using var dbContext = CreateDbContext();

            dbContext.TryDropTable<User>();
            dbContext.TryDropTable<User>();

            dbContext.TryCreateTable<User>();
            dbContext.TryCreateTable<User>();
        }
        #endregion

        #region #1 Create
        [TestMethod]
        public void Test_Create()
        {
            using var dbContext = CreateDbContext();

            var newUserList = User.NewUsers(7, 4, forAdd: true);


            // #1 Add
            dbContext.Add(newUserList[0]);

            // #2 AddRange
            dbContext.AddRange(newUserList.Skip(1));


            DataSource.WaitForUpdate();

            // assert
            {
                var userList = dbContext.Query<User>().Where(user => user.id >= 7).ToList();
                Assert.AreEqual(newUserList.Count, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
            }

            try
            {
                dbContext.Add(newUserList[0]);
                Assert.Fail("should not be able to add same key twice");
            }
            catch (Exception ex) when (ex is not AssertFailedException)
            {
            }


        }
        #endregion

        #region #2 Retrieve : Get Query
        [TestMethod]
        public void Test_Retrieve()
        {
            using var dbContext = CreateDbContext();

            // #1 Get
            {
                var user = dbContext.Get<User>(1);
                Assert.AreEqual(1, user.id);
            }

            // #2 Query
            {
                var userList = dbContext.Query<User>().ToList();
                Assert.AreEqual(6, userList.Count);
            }
        }
        #endregion


        #region #3 Update
        [TestMethod]
        public void Test_Update()
        {
            using var dbContext = CreateDbContext();

            // Update
            {
                var rowCount = dbContext.Update(User.NewUser(4));
                Assert.AreEqual(1, rowCount);
            }

            // UpdateRange
            {
                var rowCount = dbContext.UpdateRange(User.NewUsers(5, 3));
                Assert.AreEqual(2, rowCount);
            }

            DataSource.WaitForUpdate();

            // assert
            {
                var newUserList = User.NewUsers(4, 3, forAdd: false);
                var userList = dbContext.Query<User>().Where(m => m.id >= 4).ToList();
                Assert.AreEqual(newUserList.Count, userList.Count);
                Assert.AreEqual(0, userList.Select(m => m.id).Except(newUserList.Select(m => m.id)).Count());
                Assert.AreEqual(0, userList.Select(m => m.name).Except(newUserList.Select(m => m.name)).Count());
            }

        }
        #endregion


        #region #4 Delete
        [TestMethod]
        public void Test_Delete()
        {
            using var dbContext = CreateDbContext();

            // #1 Delete
            {
                var rowCount = dbContext.Delete(User.NewUser(1));
                Assert.AreEqual(1, rowCount);
            }

            // #2 DeleteRange
            {
                var rowCount = dbContext.DeleteRange(User.NewUsers(2, 2));
                Assert.AreEqual(2, rowCount);
            }

            // #3 DeleteByKey
            {
                var user = User.NewUser(4);
                var key = dbContext.GetEntityDescriptor(typeof(User)).key;
                var keyValue = key.GetValue(user);
                var rowCount = dbContext.DeleteByKey<User>(keyValue);
                Assert.AreEqual(1, rowCount);
            }

            // #4 DeleteByKeys
            {
                var users = User.NewUsers(5, 2);
                var key = dbContext.GetEntityDescriptor(typeof(User)).key;
                var keyValues = users.Select(user => key.GetValue(user));
                var rowCount = dbContext.DeleteByKeys<User, object>(keyValues);
                Assert.AreEqual(2, rowCount);
            }

            DataSource.WaitForUpdate();

            // assert
            {
                var userList = dbContext.Query<User>().ToList();
                Assert.AreEqual(0, userList.Count);
            }
        }
        #endregion


    }
}
