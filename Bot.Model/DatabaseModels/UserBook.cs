using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Bot.Model.DatabaseModels;

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

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
