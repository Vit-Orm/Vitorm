
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vit.Orm.Sqlite.MsTest
{

    [TestClass]
    public class Transaction_Test
    {

        [TestMethod]
        public void Test_Transaction()
        {
            var dbName = System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "_";
            DataSource.CreateFormatedDbContext(dbName).Dispose();


            #region Transaction
            {
                using var dbContext = DataSource.CreateDbContext(dbName);
                var userSet = dbContext.DbSet<User>();

                Assert.AreEqual("u4", userSet.Get(4).name);

                dbContext.Update(new User { id = 4, name = "u41" });
                Assert.AreEqual("u41", userSet.Get(4).name);

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u42" });
                    Assert.AreEqual("u42", userSet.Get(4).name);
                    //tran2.Rollback();
                }

                Assert.AreEqual("u41", userSet.Get(4).name);
                using (var tran3 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u43" });
                    Assert.AreEqual("u43", userSet.Get(4).name);

                    tran3.Commit();
                }

                Assert.AreEqual("u43", userSet.Get(4).name);
            }
            #endregion


            #region Transaction Dispose
            {
                {
                    using var dbContext = DataSource.CreateDbContext(dbName);
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
                    using var dbContext = DataSource.CreateDbContext(dbName);
                    var userSet = dbContext.DbSet<User>();

                    Assert.AreEqual("u42", userSet.Get(4).name);
                }
            }
            #endregion



        }




        //[TestMethod]
        public void Test_NestedTransaction()
        {
            var dbName = System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "_";

            #region
            {
                using var dbContext = DataSource.CreateFormatedDbContext(dbName);
                var userSet = dbContext.DbSet<User>();

                using (var tran1 = dbContext.BeginTransaction())
                {
                    Assert.AreEqual("u4", userSet.Get(4).name);

                    dbContext.Update(new User { id = 4, name = "u41" });
                    Assert.AreEqual("u41", userSet.Get(4).name);

                    using (var tran2 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u42" });
                        Assert.AreEqual("u42", userSet.Get(4).name);
                        tran2.Rollback();
                    }

                    Assert.AreEqual("u41", userSet.Get(4).name);
                    using (var tran3 = dbContext.BeginTransaction())
                    {
                        dbContext.Update(new User { id = 4, name = "u43" });
                        Assert.AreEqual("u43", userSet.Get(4).name);
                    }

                    Assert.AreEqual("u43", userSet.Get(4).name);
                }

                Assert.AreEqual("u43", userSet.Get(4).name);
            }
            #endregion



        }




    }
}
