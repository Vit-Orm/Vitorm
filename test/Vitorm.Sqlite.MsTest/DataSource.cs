using Vit.Extensions;
using Vitorm.Sql;

namespace Vitorm.MsTest
{
    [System.ComponentModel.DataAnnotations.Schema.Table("User")]
    public class User
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int id { get; set; }
        public string name { get; set; }
        public DateTime? birth { get; set; }

        public int? fatherId { get; set; }
        public int? motherId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string test { get; set; }


        public static User NewUser(int id, bool forAdd = false) => new User { id = id, name = "testUser" + id };

        public static List<User> NewUsers(int startId, int count = 1, bool forAdd = false)
        {
            return Enumerable.Range(startId, count).Select(id => NewUser(id, forAdd)).ToList();
        }
    }


    public class DataSource
    {
        public static void WaitForUpdate() { }

        public static SqlDbContext CreateDbContextForWriting() => CreateDbContext();
        public static SqlDbContext CreateDbContext()
        {
            var guid = Guid.NewGuid().ToString();
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"{guid}.sqlite.db");
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllBytes(filePath, new byte[0]);


            var connectionString = $"data source={filePath}";

            var dbContext = new SqlDbContext();
            dbContext.UseSqlite(connectionString);

            dbContext.BeginTransaction();

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

            return dbContext;
        }

    }
}
