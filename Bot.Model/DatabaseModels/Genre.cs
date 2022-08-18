using Bot.Model.DatabaseModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
