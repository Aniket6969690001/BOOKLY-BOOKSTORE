using System;
using System.Collections.Generic;

namespace EBook.Model;

public partial class ContactMessage
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null;

    public virtual RegistrationTable Registration { get; set; } = null!;
}
