using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vitorm.Sql;
using Vit.Extensions;
using UserEntity = Vitorm.MsTest.Sqlite.User;

namespace Vitorm.MsTest.Sqlite
{
    public class User : Vitorm.MsTest.User { }
}


namespace Vitorm.MsTest
{

    [TestClass]
    public partial class Sqlite_Test: UserTest<UserEntity>
    {

        [TestMethod]
        public void Test()
        {
            Init();

            base.Test();
        }

        public override UserEntity NewUser(int id) => new UserEntity { id = id, name = "testUser" + id };



        public static void Init()
        {

            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"sqlite.db");
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllBytes(filePath, new byte[0]);


            var connectionString = $"data source=sqlite.db";

            using var dbContext = new SqlDbContext();
            dbContext.UseSqlite(connectionString);

            var dbSet = dbContext.DbSet<User>();
            dbSet.Create();

            var users = new List<User> {
                    new User { id=1, name="u146", fatherId=4, motherId=6 },
                    new User { id=2, name="u246", fatherId=4, motherId=6 },
                    new User { id=3, name="u356", fatherId=5, motherId=6 },
                    new User { id=4, name="u400" },
                    new User { id=5, name="u500" },
                    new User { id=6, name="u600" },
                };
            users.ForEach(user => { user.birth = DateTime.Parse("2021-01-01 00:00:00").AddHours(user.id); });

            dbContext.AddRange(users);

        }


    }
}
