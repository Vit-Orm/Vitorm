using System.ComponentModel.DataAnnotations;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Property_Test
    {
        [TestMethod]
        public void Test_RequiredAndMaxLength()
        {
            using var dbContext = DataSource.CreateDbContext();
            var dbSet = dbContext.DbSet<UserInfo>();

            dbSet.TryDropTable();
            dbSet.TryCreateTable();

            {
                dbSet.Add(new UserInfo { id = 1, name = "user1" });

                try
                {
                    dbSet.Add(new UserInfo { id = 2 });
                    Assert.Fail("name should be required");
                }
                catch (Exception ex)
                { }

                try
                {
                    dbSet.Add(new UserInfo { id = 3, name = "01234567890123456789" });
                    Assert.Fail("max length of name should be 10");
                }
                catch (Exception ex)
                { }
            }
        }



        class UserInfo
        {
            [Key]
            public int id { get; set; }

            [Required]
            [MaxLength(10)]
            public string name { get; set; }
        }




    }
}
