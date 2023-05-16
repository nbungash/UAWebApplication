using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class LorryImagesTable
{
    public int Id { get; set; }

    public DateTime? Date1 { get; set; }

    public string? Description { get; set; }

    public long? AccountId { get; set; }

    public long? TripId { get; set; }

    public byte[]? Image1 { get; set; }

    public virtual AccountTable? Account { get; set; }

    public virtual TripTable? Trip { get; set; }
}
