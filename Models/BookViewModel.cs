using EBook.Model;
using System.ComponentModel.DataAnnotations;

namespace EBook.Models
{
    public class BooksListViewModel
    {
        public List<BookViewModel> Books { get; set; } = new List<BookViewModel>();
        public List<Genre>? Genres { get; set; } = new List<Genre>();
        // Used for the Add/Edit modal form
        public BookViewModel Book { get; set; } = new BookViewModel();

    }

    public class BookViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Book name is required.")]
        public string BookName { get; set; }

        [Required(ErrorMessage = "Author name is required.")]
        public string AuthorName { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public Double Price { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        public int? GenreId { get; set; }

        public string? GenreName { get; set; }
        public string? Image { get; set; }
        public string? SoftCopy { get; set; }


    }
}
