using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Model.DatabaseModels
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        public long UserId { get; set; }

        public string Username { get; set; } = "";

        public List<UserBook> UsersBooks { get; set; } = new();
    }
}
