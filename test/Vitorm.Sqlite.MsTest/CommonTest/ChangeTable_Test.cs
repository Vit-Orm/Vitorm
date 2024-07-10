using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public partial class ChangeTable_Test
    {

        [TestMethod]
        public void Test_ChangeTable()
        {
            string tableName;

            using var dbContext = DataSource.CreateDbContextForWriting();

            var dbSet = dbContext.DbSet<User>();
            tableName = dbSet.entityDescriptor.tableName;
            User user;

            // User
            {
                user = dbSet.Get(1);
                Assert.IsNotNull(user);
            }

            // User2
            {
                dbSet.ChangeTable(tableName + "2");

                dbSet.Drop();
                dbSet.Create();

                user = dbSet.Get(1);
                Assert.IsNull(user);

                var users = User.NewUsers(startId: 1, count: 5, forAdd: true);
                user = users[1];
                user.name = "Hello User2";
                dbSet.AddRange(users);

                dbSet.DeleteByKey(1);

            }

            DataSource.WaitForUpdate();

            // Assert User2
            {
                var userList = dbSet.Query().Where(user => new[] { 1, 2, 3 }.Contains(user.id)).OrderBy(u => u.id).ToList();

                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual(2, userList[0].id);
                Assert.AreEqual("Hello User2", userList[0].name);
            }

            // Assert User
            {
                //dbSet.ChangeTableBack();
                dbSet.ChangeTable(tableName);
                Assert.AreEqual("u246", dbSet.Get(2)?.name);
            }

        }



    }
}
