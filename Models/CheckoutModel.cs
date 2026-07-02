using System.ComponentModel.DataAnnotations;

namespace EBook.Models
{
    public class CheckoutModel
    {
        // User info (optional if logged in)
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        // Shipping Address
        [Required]
        [Display(Name = "Address Line 1")]
        public string Address { get; set; }

       

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        // Payment details (optional if using cash-on-delivery or simple orders)
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        // Cart Summary
        public double TotalAmount { get; set; }


        public List<CartItem> CartItems { get; set; }
    }
}
