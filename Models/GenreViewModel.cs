using EBook.Model;

namespace EBook.Models
{

    public class GenreWithCount
    {
        public int Id { get; set; }

        public string GenreName { get; set; } = null!;
        public int BookCount { get; set; }
    }
    public class GenreViewModel
    {
        public List<GenreWithCount> Genres { get; set; } = new List<GenreWithCount>();
        public Genre NewGenre { get; set; } = new Genre();
    }
}
