using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Model.DatabaseModels
{
    [Table("UsersBooks")]
    public class UserBook
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
