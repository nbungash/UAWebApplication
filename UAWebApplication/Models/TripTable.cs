using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class TripTable
{
    public long TripId { get; set; }

    public long? PartyId { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? TokenNo { get; set; }

    public long? Lorry { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public long? ShippingId { get; set; }

    public long? DestinationId { get; set; }

    public long? ProductId { get; set; }

    public double? Quantity { get; set; }

    public string? QtyUnit { get; set; }

    public double? Rate { get; set; }

    public decimal? Freight { get; set; }

    public decimal? CommissionPercent { get; set; }

    public decimal? Commission { get; set; }

    public decimal? TaxPercent { get; set; }

    public decimal? Tax { get; set; }

    public double? ShortQty { get; set; }

    public decimal? ShortRate { get; set; }

    public decimal? ShortAmount { get; set; }

    public long? PartyBillId { get; set; }

    public long? LorryBillNo { get; set; }

    public long? UserId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int? SummaryId { get; set; }

    public int? SummaryShortId { get; set; }

    public decimal? SummaryShort { get; set; }

    public virtual DestinationTable? Destination { get; set; }

    public virtual ICollection<JournalTable> JournalTables { get; } = new List<JournalTable>();

    public virtual LorryBillTable? LorryBillNoNavigation { get; set; }

    public virtual ICollection<LorryImagesTable> LorryImagesTables { get; } = new List<LorryImagesTable>();

    public virtual AccountTable? LorryNavigation { get; set; }

    public virtual AccountTable? Party { get; set; }

    public virtual PartyBillTable? PartyBill { get; set; }

    public virtual ProductTable? Product { get; set; }

    public virtual ShippingTable? Shipping { get; set; }

    public virtual PsosummaryTable? Summary { get; set; }

    public virtual PsosummaryTable? SummaryShortNavigation { get; set; }

    public virtual LoginTable? User { get; set; }
}
