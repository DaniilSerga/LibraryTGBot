using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Model.DatabaseModels
{
    [Table("Authors")]
    public class Author
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public List<Book>? Book { get; set; }
    }
}
