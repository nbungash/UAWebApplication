using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class ProvincesTable
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? InterProvinceSalesTax { get; set; }

    public decimal? IntraProvinceSalesTax { get; set; }

    public virtual ICollection<SalesTaxInvoicesTable> SalesTaxInvoicesTableDestinationProvinces { get; } = new List<SalesTaxInvoicesTable>();

    public virtual ICollection<SalesTaxInvoicesTable> SalesTaxInvoicesTableInvoiceProvinces { get; } = new List<SalesTaxInvoicesTable>();

    public virtual ICollection<SalesTaxInvoicesTable> SalesTaxInvoicesTableShippingProvinces { get; } = new List<SalesTaxInvoicesTable>();
}
