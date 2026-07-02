using System;
using System.Collections.Generic;

namespace EBook.Model;

public partial class UserDetail
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int RegistrationId { get; set; }

    public string Address { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Pincode { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public virtual RegistrationTable Registration { get; set; } = null!;
}
