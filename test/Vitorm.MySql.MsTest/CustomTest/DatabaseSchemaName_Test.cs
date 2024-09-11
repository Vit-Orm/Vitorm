using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CustomTest
{

    [TestClass]
    public partial class DatabaseSchemaName_Test
    {

        [TestMethod]
        public void Test()
        {
            using var dbContext = DataSource.CreateDbContextForWriting();

            dbContext.TryDropTable<User2>();
            dbContext.TryCreateTable<User2>();

            dbContext.Add(new User2 { name = "test" });


            // assert
            {
                var user = dbContext.Get<User>(1);
                Assert.AreEqual("u146", user.name);
            }
            {
                var user = dbContext.Get<User2>(1);
                Assert.AreEqual("test", user.name);
            }

        }



        [System.ComponentModel.DataAnnotations.Schema.Table("User", Schema = "orm")]
        public class User2 : User
        { }


    }
}
