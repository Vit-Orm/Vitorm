namespace App.Runner
{

    // Entity Definition
    [System.ComponentModel.DataAnnotations.Schema.Table("User")]
    public class User
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int id { get; set; }
        public string name { get; set; }
        public DateTime? birth { get; set; }
        public int? fatherId { get; set; }
        public int? motherId { get; set; }
    }
}