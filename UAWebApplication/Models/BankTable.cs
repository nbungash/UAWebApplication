using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class BankTable
{
    public long Id { get; set; }

    public string? BankName { get; set; }

    public string? AccountNo { get; set; }

    public string? BankCode { get; set; }

    public string? Address { get; set; }

    public long? AccountId { get; set; }

    public virtual AccountTable? Account { get; set; }
}
