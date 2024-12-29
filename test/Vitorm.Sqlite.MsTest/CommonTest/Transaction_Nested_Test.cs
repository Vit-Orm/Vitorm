using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Transaction_Nested_Test
    {

        [TestMethod]
        public void Test_NestedTransaction()
        {
            {
                using var dbContext = DataSource.CreateDbContext();
                var userSet = dbContext.DbSet<User>();

                Assert.AreEqual("u400", userSet.Get(4).name);

                using (var tran1 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4001" });
                    Assert.AreEqual("u4001", userSet.Get(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u4002" });
                        Assert.AreEqual("u4002", userSet.Get(4).name);
                    }
                    Assert.AreEqual("u4001", userSet.Get(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u4002" });
                        Assert.AreEqual("u4002", userSet.Get(4).name);
                        tran2.Rollback();
                    }
                    Assert.AreEqual("u4001", userSet.Get(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u4003" });
                        Assert.AreEqual("u4003", userSet.Get(4).name);
                        tran2.Commit();
                    }
                    Assert.AreEqual("u4003", userSet.Get(4).name);
                }

                Assert.AreEqual("u400", userSet.Get(4).name);
            }
        }


    }
}
