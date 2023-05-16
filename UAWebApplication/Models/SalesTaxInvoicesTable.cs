using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class SalesTaxInvoicesTable
{
    public long Id { get; set; }

    public string? InvoiceNo { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public decimal? SalesTaxPercent { get; set; }

    public int? ShippingProvinceId { get; set; }

    public int? DestinationProvinceId { get; set; }

    public int? InvoiceProvinceId { get; set; }

    public long? PartyBillId { get; set; }

    public long? PartyId { get; set; }

    public virtual ProvincesTable? DestinationProvince { get; set; }

    public virtual ProvincesTable? InvoiceProvince { get; set; }

    public virtual AccountTable? Party { get; set; }

    public virtual PartyBillTable? PartyBill { get; set; }

    public virtual ProvincesTable? ShippingProvince { get; set; }
}
