namespace EBook.Models
{

    public class OrderWithBookViewModel
    {
        public int OrderId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsOrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BookInOrderViewModel> Books { get; set; }
    }

    public class BookInOrderViewModel
    {
        public string BookTitle { get; set; }
        public string Genre { get; set; }
        public string Image { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}