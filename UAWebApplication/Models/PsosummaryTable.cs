using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class PsosummaryTable
{
    public int Id { get; set; }

    public long? CompanyId { get; set; }

    public DateTime? SummaryDate { get; set; }

    public decimal? Freight { get; set; }

    public decimal? Tax { get; set; }

    public decimal? ShortAmount { get; set; }

    public decimal? PenaltyAmount { get; set; }

    public decimal? OnlineAmount { get; set; }

    public long? BankId { get; set; }

    public decimal? CreditAmount { get; set; }

    public bool? IsFinalized { get; set; }

    public virtual AccountTable? Bank { get; set; }

    public virtual AccountTable? Company { get; set; }

    public virtual ICollection<JournalTable> JournalTables { get; } = new List<JournalTable>();

    public virtual ICollection<PartyBillTable> PartyBillTables { get; } = new List<PartyBillTable>();

    public virtual ICollection<TripTable> TripTableSummaries { get; } = new List<TripTable>();

    public virtual ICollection<TripTable> TripTableSummaryShortNavigations { get; } = new List<TripTable>();
}
