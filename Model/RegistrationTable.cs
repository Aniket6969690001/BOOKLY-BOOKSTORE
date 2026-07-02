using System;
using System.Collections.Generic;

namespace EBook.Model;

public partial class RegistrationTable
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? ProfileImage { get; set; }

    public bool IsAdmin { get; set; }

    public string? FullName { get; set; }

    public string? Mobile { get; set; }

    public string? Address { get; set; }

    public string? Pincode { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
