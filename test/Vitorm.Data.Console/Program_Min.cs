using Vitorm;
namespace App
{
    public class Program_Min
    {
        static void Main2(string[] args)
        {
            //  Query Records
            var user = Data.Get<User>(1);
            var users = Data.Query<User>().Where(u => u.name.Contains("li")).ToList();
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