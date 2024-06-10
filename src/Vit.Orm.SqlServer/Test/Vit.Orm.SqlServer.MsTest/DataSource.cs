using Dapper.Contrib.Extensions;

using Vit.Orm.Sql;
using Vit.Extensions;
using Vit.Core.Util.ConfigurationManager;

namespace Vit.Orm.MsTest
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
        static string connectionString = Appsettings.json.GetStringByPath("App.Db.ConnectionString");

        public static SqlDbContext CreateDbContext()
        {
            var dbContext = new SqlDbContext();
            dbContext.UseSqlServer(connectionString);

            dbContext.BeginTransaction();

            var userSet = dbContext.DbSet<User>();

            dbContext.Execute(sql: "IF OBJECT_ID(N'User', N'U') IS  NOT  NULL \r\nDROP TABLE [User];");

            userSet.Create();

            var users = new List<User> {
                    new User {   name="u1", fatherId=4, motherId=6 },
                    new User {   name="u2", fatherId=4, motherId=6 },
                    new User {   name="u3", fatherId=5, motherId=6 },
                    new User {   name="u4" },
                    new User {   name="u5" },
                    new User {   name="u6" },
                };

            dbContext.AddRange(users);

            users.ForEach(user => { user.birth = DateTime.Parse("2021-01-01 00:00:00").AddHours(user.id); });

            dbContext.UpdateRange(users);

            return dbContext;
        }

    }
}
