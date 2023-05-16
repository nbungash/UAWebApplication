using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class ResourceTable
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public virtual ICollection<AspNetRole> AspNetRoles { get; } = new List<AspNetRole>();
}
