namespace EBook.Models
{
    public class OrderConfirmationModel
    {
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
}
