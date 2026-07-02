using System;
using System.Collections.Generic;

namespace EBook.Model;

public partial class Wishlist
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int RegistrationId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual RegistrationTable Registration { get; set; } = null!;
}
