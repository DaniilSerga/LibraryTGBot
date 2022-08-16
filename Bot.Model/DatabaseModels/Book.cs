using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Model.DatabaseModels
{
    [Table("Books")]
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public string Link { get; set; } = "";

        public string Author { get; set; } = "";

        public string Genre { get; set; } = "";

        public string PictureLink { get; set; } = "";
    }
}
