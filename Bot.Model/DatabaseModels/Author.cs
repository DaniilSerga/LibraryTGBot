using Bot.Model.DatabaseModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
