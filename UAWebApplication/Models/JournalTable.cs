using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class JournalTable
{
    public long Id { get; set; }

    public long? TripId { get; set; }

    public long? TransId { get; set; }

    public DateTime? EntryDate { get; set; }

    public long? AccountId { get; set; }

    public decimal? Credit { get; set; }

    public decimal? Debit { get; set; }

    public string? EntryType { get; set; }

    public string? Description { get; set; }

    public string? ChequeNo { get; set; }

    public long? LorryBillNo { get; set; }

    public long? PartyBillId { get; set; }

    public long? UserId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public long? VoucherNo { get; set; }

    public bool? IsChecked { get; set; }

    public int? CloseId { get; set; }

    public string? ReceiverName { get; set; }

    public int? SummaryId { get; set; }

    public string? Lorry { get; set; }

    public long? PumpTransId { get; set; }

    public decimal? Quanitity { get; set; }

    public virtual AccountTable? Account { get; set; }

    public virtual IsclosingTable? Close { get; set; }

    public virtual LorryBillTable? LorryBillNoNavigation { get; set; }

    public virtual PartyBillTable? PartyBill { get; set; }

    public virtual PsosummaryTable? Summary { get; set; }

    public virtual TripTable? Trip { get; set; }

    public virtual LoginTable? User { get; set; }
}
