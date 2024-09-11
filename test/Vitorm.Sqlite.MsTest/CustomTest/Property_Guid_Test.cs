using System.ComponentModel.DataAnnotations;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CustomTest
{

    [TestClass]
    public class Property_Guid_Test
    {

        [TestMethod]
        public void Test()
        {
            using var dbContext = DataSource.CreateDbContext();
            var dbSet = dbContext.DbSet<UserInfo>();

            dbSet.TryDropTable();
            dbSet.TryCreateTable();

            Guid guid; UserInfo user;
            {
                user = dbSet.Add(new UserInfo { name = "user1" });
                user = dbSet.Add(new UserInfo { name = "user2" });
                guid = user.guid;
            }

            DataSource.WaitForUpdate();

            {
                user = dbSet.Get(guid);
                Assert.AreEqual("user2", user?.name);
            }
        }
        class UserInfo
        {
            [Key]
            public Guid guid { get; set; } = Guid.NewGuid();
            public string name { get; set; }
        }






        [TestMethod]
        public void Test2()
        {
            using var dbContext = DataSource.CreateDbContext();
            var dbSet = dbContext.DbSet<UserInfo2>();

            dbSet.TryDropTable();
            dbSet.TryCreateTable();

            Guid guid = Guid.NewGuid();
            UserInfo2 user;

            {
                user = dbSet.Add(new UserInfo2 { guid = guid, name = "user1" });

                DataSource.WaitForUpdate();

                try
                {
                    dbSet.Add(new UserInfo2 { guid = guid, name = "user1" });
                    Assert.Fail("should not be able to add same key twice");
                }
                catch (Exception ex)
                { }

                try
                {
                    user = dbSet.Add(new UserInfo2 { name = "user2" });
                    Assert.Fail("should not be able to add entity without key");
                }
                catch (Exception ex)
                { }
            }

            {
                user = dbSet.Get(guid);
                Assert.AreEqual("user1", user?.name);
            }
        }

        class UserInfo2
        {
            [Key]
            public Guid guid { get; set; }

            public string name { get; set; }
        }




    }
}
