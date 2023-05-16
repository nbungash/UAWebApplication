using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class ProductTable
{
    public string? Title { get; set; }

    public string? TitleUrdu { get; set; }

    public long Id { get; set; }

    public string? ProductCode { get; set; }

    public long? PartyId { get; set; }

    public virtual AccountTable? Party { get; set; }

    public virtual ICollection<TripTable> TripTables { get; } = new List<TripTable>();
}
