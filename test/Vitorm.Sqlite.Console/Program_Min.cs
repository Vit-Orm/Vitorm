using Vitorm;
namespace App
{
    public class Program_Min
    {
        static void Main2(string[] args)
        {
            // #1 Init
            using var dbContext = new Vitorm.Sql.SqlDbContext();
            dbContext.UseSqlite("data source=sqlite.db");

            // #2 Query
            var user = dbContext.Get<User>(1);
            var users = dbContext.Query<User>().Where(u => u.name.Contains("li")).ToList();
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