using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class LorryTable
{
    public int Id { get; set; }

    public string? Capacity { get; set; }

    public string? OwnerName { get; set; }

    public string? OwnerNameInUrdu { get; set; }

    public long? AccountId { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public string? ChassisNo { get; set; }

    public string? EngineNo { get; set; }

    public decimal? CommissionPercent { get; set; }

    public decimal? TaxPercent { get; set; }

    public DateTime? DipChartDueDate { get; set; }

    public DateTime? TrackerDueDate { get; set; }

    public DateTime? TokenDueDate { get; set; }

    public virtual AccountTable? Account { get; set; }
}
