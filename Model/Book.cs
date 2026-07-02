using System;
using System.Collections.Generic;

namespace EBook.Model;

public partial class Book
{
    public int Id { get; set; }

    public string BookName { get; set; } = null!;

    public string AuthorName { get; set; } = null!;

    public double Price { get; set; }

    public string? Image { get; set; }

    public string? SoftCopy { get; set; }

    public string? Description { get; set; }

    public int? GenreId { get; set; }

    public bool? IsBestSelling { get; set; }

    public bool? IsRecommended { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? BookReviews { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Genre? Genre { get; set; }

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
