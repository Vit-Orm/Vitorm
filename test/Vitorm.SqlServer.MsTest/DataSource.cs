using Vitorm.Sql;
using Vit.Extensions;
using Vit.Core.Util.ConfigurationManager;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vitorm.MsTest
{
    [System.ComponentModel.DataAnnotations.Schema.Table("User", Schema = "dbo")]
    public class User
    {
        [System.ComponentModel.DataAnnotations.Key]
        [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Schema.Column("id", TypeName = "int")]
        public int id { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column("name", TypeName = "varchar(1000)")]
        [System.ComponentModel.DataAnnotations.Required]
        public string name { get; set; }
     
        public DateTime? birth { get; set; }

        public int? fatherId { get; set; }
        public int? motherId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string test{ get; set; }

        public static User NewUser(int id, bool forAdd = false) => new User { id = forAdd ? 0 : id, name = "testUser" + id };

        public static List<User> NewUsers(int startId, int count = 1, bool forAdd = false)
        {
            return Enumerable.Range(startId, count).Select(id => NewUser(id, forAdd)).ToList();
        }
    }


    public class DataSource
    {
        public static void WaitForUpdate() { }

        static string connectionString = Appsettings.json.GetStringByPath("Vitorm.SqlServer.connectionString");

        public static SqlDbContext CreateDbContextForWriting() => CreateDbContext();

        public static SqlDbContext CreateDbContext()
        {
            var dbContext = new SqlDbContext();
            dbContext.UseSqlServer(connectionString);

            dbContext.BeginTransaction();

            dbContext.Execute(sql: "IF OBJECT_ID(N'User', N'U') IS  NOT  NULL \r\nDROP TABLE [User];");

            dbContext.Create<User>();

            var users = new List<User> {
                    new User {   name="u146", fatherId=4, motherId=6 },
                    new User {   name="u246", fatherId=4, motherId=6 },
                    new User {   name="u356", fatherId=5, motherId=6 },
                    new User {   name="u400" },
                    new User {   name="u500" },
                    new User {   name="u600" },
                };

            dbContext.AddRange(users);

            users.ForEach(user => { user.birth = DateTime.Parse("2021-01-01 00:00:00").AddHours(user.id); });

            dbContext.UpdateRange(users);

            return dbContext;
        }

    }
}
