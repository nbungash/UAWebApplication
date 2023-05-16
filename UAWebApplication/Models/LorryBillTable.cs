using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class LorryBillTable
{
    public long BillNo { get; set; }

    public DateTime? BillDate { get; set; }

    public string? BillDateString { get; set; }

    public long? Lorry { get; set; }

    public string? OwnerName { get; set; }

    public decimal? BillCharges { get; set; }

    public long? UserId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public virtual ICollection<JournalTable> JournalTables { get; } = new List<JournalTable>();

    public virtual ICollection<LorryBillPrintTable> LorryBillPrintTables { get; } = new List<LorryBillPrintTable>();

    public virtual AccountTable? LorryNavigation { get; set; }

    public virtual ICollection<TripTable> TripTables { get; } = new List<TripTable>();

    public virtual LoginTable? User { get; set; }
}
