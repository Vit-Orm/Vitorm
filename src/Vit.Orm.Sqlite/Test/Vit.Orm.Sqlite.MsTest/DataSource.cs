using Dapper.Contrib.Extensions;

using Vit.Orm.Sql;
using Vit.Extensions;

namespace Vit.Orm.Sqlite.MsTest
{
    [Table("User")]
    public class User
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string name { get; set; }
        public DateTime? birth { get; set; }

        public int? fatherId { get; set; }
        public int? motherId { get; set; }
    }


    public class DataSource
    {
        public static DbContext CreateDbContext(string dbName = "DataSource")
        {
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"{dbName}.db");
            var dbContext = new SqlDbContext();
            dbContext.UseSqlite($"data source={filePath}");
            return dbContext;
            //return CreateFormatedDbContext(dbName);
        }

        public static DbContext CreateFormatedDbContext(string dbName = "DataSource")
        {
            var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"{dbName}.db");
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllBytes(filePath, new byte[0]);


            var connectionString = $"data source={filePath}";
            //connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";
            //connectionString = $"Data Source=:memory:";

            var dbContext = new SqlDbContext();
            dbContext.UseSqlite(connectionString);

            var userSet = dbContext.DbSet<User>();
            userSet.Create();

            var users = new List<User> {
                    new User { id=1, name="u1", fatherId=4, motherId=6 },
                    new User { id=2, name="u2", fatherId=4, motherId=6 },
                    new User { id=3, name="u3", fatherId=5, motherId=6 },
                    new User { id=4, name="u4" },
                    new User { id=5, name="u5" },
                    new User { id=6, name="u6" },
                };
            users.ForEach(user => { user.birth = DateTime.Parse("2021-01-01 00:00:00").AddHours(user.id); });

            dbContext.AddRange(users);

            return dbContext;
        }

    }
}
