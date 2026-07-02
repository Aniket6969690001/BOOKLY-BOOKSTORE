namespace EBook.Models
{
    public class CartItem
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string AuthorName { get; set; }
        public string? Image { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}
