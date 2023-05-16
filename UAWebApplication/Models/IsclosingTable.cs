using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class IsclosingTable
{
    public int Id { get; set; }

    public DateTime? Date1 { get; set; }

    public decimal? TotalRevenue { get; set; }

    public decimal? TotalExpense { get; set; }

    public decimal? Income { get; set; }

    public virtual ICollection<JournalTable> JournalTables { get; } = new List<JournalTable>();
}
