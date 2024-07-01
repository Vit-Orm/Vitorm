using Vit.Extensions.Vitorm_Extensions;
using Vitorm;

namespace App
{
    public class Program
    {
        static void Main(string[] args)
        {
            // #1 Create an empty SQLite database file and configures Vitorm
            File.WriteAllBytes("sqlite.db", new byte[0]);
            using var dbContext = new Vitorm.Sql.SqlDbContext();
            dbContext.UseSqlite("data source=sqlite.db");

            // #2 Create Table
            dbContext.Drop<User>();
            dbContext.Create<User>();

            // #3 Insert Records
            dbContext.Add(new User { id = 1, name = "lith" });
            dbContext.AddRange(new[] {
                new User { id = 2, name = "lith", fatherId = 1 },
                new User { id = 3, name = "lith", fatherId = 1 }
            });

            // #4 Query Records
            {
                var user = dbContext.Get<User>(1);
                var users = dbContext.Query<User>().Where(u => u.name.Contains("li")).ToList();
                var sql = dbContext.Query<User>().Where(u => u.name.Contains("li")).ToExecuteString();
            }

            // #5 Update Records
            dbContext.Update(new User { id = 1, name = "lith1" });
            dbContext.UpdateRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            dbContext.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteUpdate(u => new User { name = "Lith" + u.id });

            // #6 Delete Records
            dbContext.Delete<User>(new User { id = 1, name = "lith1" });
            dbContext.DeleteRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            dbContext.DeleteByKey<User>(1);
            dbContext.DeleteByKeys<User, int>(new[] { 1, 2 });
            dbContext.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteDelete();

            // #7 Join Queries
            {
                var query =
                        from user in dbContext.Query<User>()
                        from father in dbContext.Query<User>().Where(father => user.fatherId == father.id).DefaultIfEmpty()
                        where father != null
                        orderby user.id
                        select new { user, father };

                var sql = query.ToExecuteString();
                var users = query.ToList();
            }

            // #8 Transactions
            {
                using var tran1 = dbContext.BeginTransaction();
                dbContext.Update(new User { id = 4, name = "u4001" });

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4002" });
                    // will rollback
                }

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4002" });
                    tran2.Rollback();
                }

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4003" });
                    tran2.Commit();
                }

                tran1.Commit();
            }

            // #9 Database Functions
            {
                // select * from User where IIF(t0.fatherId is not null, true, false);
                var query = dbContext.Query<User>().Where(u => DbFunction.Call<bool>("IIF", u.fatherId != null, true, false));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
            }
        }

        // Entity Definition
        [System.ComponentModel.DataAnnotations.Schema.Table("User")]
        public class User
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int id { get; set; }
            public string name { get; set; }
            public DateTime? birth { get; set; }
            public int? fatherId { get; set; }
        }
    }
}