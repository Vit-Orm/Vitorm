
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Transaction_Test
    {

        [TestMethod]
        public void Test_Transaction()
        {
            #region Transaction
            {
                using var dbContext = DataSource.CreateDbContext();
                var userSet = dbContext.DbSet<User>();

                Assert.AreEqual("u4", userSet.Get(4).name);

                dbContext.Update(new User { id = 4, name = "u41" });
                Assert.AreEqual("u41", userSet.Get(4).name);

                using (var tran = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u42" });
                    Assert.AreEqual("u42", userSet.Get(4).name);
                }
                Assert.AreEqual("u41", userSet.Get(4).name);

                using (var tran = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u42" });
                    Assert.AreEqual("u42", userSet.Get(4).name);
                    tran.Rollback();
                }
                Assert.AreEqual("u41", userSet.Get(4).name);

                using (var tran = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u43" });
                    Assert.AreEqual("u43", userSet.Get(4).name);
                    tran.Commit();
                }
                Assert.AreEqual("u43", userSet.Get(4).name);

            }
            #endregion
        }


        // can not test for db is not durable
        //[TestMethod]
        public void Test_Dispose()
        {
            {
                using var dbContext = DataSource.CreateDbContext();
                var userSet = dbContext.DbSet<User>();

                var tran2 = dbContext.BeginTransaction();
                {
                    dbContext.Update(new User { id = 4, name = "u42" });
                    Assert.AreEqual("u42", userSet.Get(4).name);
                    tran2.Commit();
                }

                Assert.AreEqual("u42", userSet.Get(4).name);

                var tran3 = dbContext.BeginTransaction();
                {
                    dbContext.Update(new User { id = 4, name = "u43" });
                    Assert.AreEqual("u43", userSet.Get(4).name);
                }
                Assert.AreEqual("u43", userSet.Get(4).name);
            }
            {
                using var dbContext = DataSource.CreateDbContext();
                var userSet = dbContext.DbSet<User>();

                //Assert.AreEqual("u42", userSet.Get(4).name);
            }

        }

        [TestMethod]
        public void Test_NestedTransaction()
        {
            #region NestedTransaction
            {
                using var dbContext = DataSource.CreateDbContext();
                var userSet = dbContext.DbSet<User>();

                Assert.AreEqual("u4", userSet.Get(4).name);

                using (var tran1 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u41" });
                    Assert.AreEqual("u41", userSet.Get(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u42" });
                        Assert.AreEqual("u42", userSet.Get(4).name);
                    }
                    Assert.AreEqual("u41", userSet.Get(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u42" });
                        Assert.AreEqual("u42", userSet.Get(4).name);
                        tran2.Rollback();
                    }
                    Assert.AreEqual("u41", userSet.Get(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u43" });
                        Assert.AreEqual("u43", userSet.Get(4).name);
                        tran2.Commit();
                    }
                    Assert.AreEqual("u43", userSet.Get(4).name);
                }

                Assert.AreEqual("u4", userSet.Get(4).name);
            }
            #endregion
        }




    }
}
