using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class LorryBillPrintTable
{
    public int Id { get; set; }

    public long? LorryBill { get; set; }

    public int? PrintCounter { get; set; }

    public virtual LorryBillTable? LorryBillNavigation { get; set; }
}
