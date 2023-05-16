using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class AccountContactTable
{
    public long AccountContactId { get; set; }

    public long? AccountId { get; set; }

    public string? ContactNo { get; set; }

    public string? Name { get; set; }

    public virtual AccountTable? Account { get; set; }
}
