using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Model.DatabaseModels
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public List<Book>? Books { get; set; } = new();
    }
}
