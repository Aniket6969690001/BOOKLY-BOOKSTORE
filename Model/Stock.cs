using System;
using System.Collections.Generic;

namespace EBook.Model;

public partial class Stock
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int Quantity { get; set; }

    public virtual Book Book { get; set; } = null!;
}
