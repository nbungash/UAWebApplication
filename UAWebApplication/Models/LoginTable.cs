using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class LoginTable
{
    public long Id { get; set; }

    public string? UserId { get; set; }

    public string? Password { get; set; }

    public string UserName { get; set; } = null!;

    public string? Role { get; set; }

    public virtual ICollection<AccountTable> AccountTables { get; } = new List<AccountTable>();

    public virtual ICollection<JournalTable> JournalTables { get; } = new List<JournalTable>();

    public virtual ICollection<LorryBillTable> LorryBillTables { get; } = new List<LorryBillTable>();

    public virtual ICollection<PartyBillTable> PartyBillTables { get; } = new List<PartyBillTable>();

    public virtual ICollection<TripTable> TripTables { get; } = new List<TripTable>();
}
