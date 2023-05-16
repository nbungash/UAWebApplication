using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class PartyBillTable
{
    public long Id { get; set; }

    public string BillNo { get; set; } = null!;

    public DateTime? BillDate { get; set; }

    public int? SummaryId { get; set; }

    public long? UserId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? ShippingProvince { get; set; }

    public string? ShippingCity { get; set; }

    public string? DestinationProvince { get; set; }

    public string? DestinationCity { get; set; }

    public long? PartyId { get; set; }

    public bool? IsFinalized { get; set; }

    public virtual ICollection<JournalTable> JournalTables { get; } = new List<JournalTable>();

    public virtual AccountTable? Party { get; set; }

    public virtual ICollection<SalesTaxInvoicesTable> SalesTaxInvoicesTables { get; } = new List<SalesTaxInvoicesTable>();

    public virtual PsosummaryTable? Summary { get; set; }

    public virtual ICollection<TripTable> TripTables { get; } = new List<TripTable>();

    public virtual LoginTable? User { get; set; }
}
