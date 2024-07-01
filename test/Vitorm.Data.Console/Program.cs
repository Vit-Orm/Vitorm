using Vit.Extensions.Vitorm_Extensions;
using Vitorm;

namespace App
{
    public class Program
    {
        static void Main(string[] args)
        {
            // #1 Create an empty SQLite database file and configures Vitorm.Data
            File.WriteAllBytes("sqlite.db", new byte[0]);

            // #2 Create Table
            Data.Drop<User>();
            Data.Create<User>();

            // #3 Insert Records
            Data.Add(new User { id = 1, name = "lith" });
            Data.AddRange(new[] {
                new User { id = 2, name = "lith", fatherId = 1 },
                new User { id = 3, name = "lith", fatherId = 1 }
            });

            // #4 Query Records
            {
                var user = Data.Get<User>(1);
                var users = Data.Query<User>().Where(u => u.name.Contains("li")).ToList();
                var sql = Data.Query<User>().Where(u => u.name.Contains("li")).ToExecuteString();
            }

            // #5 Update Records
            Data.Update(new User { id = 1, name = "lith1" });
            Data.UpdateRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            Data.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteUpdate(u => new User { name = "Lith" + u.id });

            // #6 Delete Records
            Data.Delete<User>(new User { id = 1, name = "lith1" });
            Data.DeleteRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            Data.DeleteByKey<User>(1);
            Data.DeleteByKeys<User, int>(new[] { 1, 2 });
            Data.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteDelete();

            // #7 Join Queries
            {
                var query =
                        from user in Data.Query<User>()
                        from father in Data.Query<User>().Where(father => user.fatherId == father.id).DefaultIfEmpty()
                        where father != null
                        orderby user.id
                        select new { user, father };

                var sql = query.ToExecuteString();
                var users = query.ToList();
            }

            // #8 Transactions
            {
                using var dbContext = Data.DataProvider<User>().CreateSqlDbContext();
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
                var query = Data.Query<User>().Where(u => DbFunction.Call<bool>("IIF", u.fatherId != null, true, false));
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