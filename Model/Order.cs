using System;
using System.Collections.Generic;

namespace EBook.Model;

public partial class Order
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }

    public DateTime CreateDate { get; set; }

    public bool IsOrderStatus { get; set; }

    public bool IsDelete { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public bool IsPaid { get; set; }

    public int BookId { get; set; }

    public virtual RegistrationTable Registration { get; set; } = null!;
    public virtual Book Books { get; set; }
}
