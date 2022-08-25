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

        public string PictureLink { get; set; } = "";

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public int GenreId { get; set; }
        public Genre? Genre { get; set; }

        public List<UserBook> UsersBooks { get; set; } = new();
    }
}
