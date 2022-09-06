using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Model.DatabaseModels
{
    [Table("Genres")]
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public List<Book>? Book { get; set; }
    }
}
