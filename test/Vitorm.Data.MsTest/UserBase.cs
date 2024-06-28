
namespace Vitorm.MsTest
{

    [System.ComponentModel.DataAnnotations.Schema.Table("User")]
    public class UserBase
    {
        [System.ComponentModel.DataAnnotations.Key]
        public virtual int id { get; set; }
        public string name { get; set; }
        public DateTime? birth { get; set; }

        public int? fatherId { get; set; }
        public int? motherId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string test { get; set; }
    }

}
