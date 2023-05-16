using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class AccountTable
{
    public long AccountId { get; set; }

    public string Title { get; set; } = null!;

    public string? TitleUrdu { get; set; }

    public string? AccountType { get; set; }

    public string? GroupType { get; set; }

    public bool? Record { get; set; }

    public long? UserId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public bool? Marked { get; set; }

    public virtual ICollection<AccountContactTable> AccountContactTables { get; } = new List<AccountContactTable>();

    public virtual ICollection<BankTable> BankTables { get; } = new List<BankTable>();

    public virtual ICollection<DestinationTable> DestinationTables { get; } = new List<DestinationTable>();

    public virtual ICollection<JournalTable> JournalTables { get; } = new List<JournalTable>();

    public virtual ICollection<LorryBillTable> LorryBillTables { get; } = new List<LorryBillTable>();

    public virtual ICollection<LorryImagesTable> LorryImagesTables { get; } = new List<LorryImagesTable>();

    public virtual ICollection<LorryTable> LorryTables { get; } = new List<LorryTable>();

    public virtual ICollection<PartyBillTable> PartyBillTables { get; } = new List<PartyBillTable>();

    public virtual ICollection<ProductTable> ProductTables { get; } = new List<ProductTable>();

    public virtual ICollection<PsosummaryTable> PsosummaryTableBanks { get; } = new List<PsosummaryTable>();

    public virtual ICollection<PsosummaryTable> PsosummaryTableCompanies { get; } = new List<PsosummaryTable>();

    public virtual ICollection<SalesTaxInvoicesTable> SalesTaxInvoicesTables { get; } = new List<SalesTaxInvoicesTable>();

    public virtual ICollection<ShippingTable> ShippingTables { get; } = new List<ShippingTable>();

    public virtual ICollection<TripTable> TripTableLorryNavigations { get; } = new List<TripTable>();

    public virtual ICollection<TripTable> TripTableParties { get; } = new List<TripTable>();

    public virtual LoginTable? User { get; set; }
}
