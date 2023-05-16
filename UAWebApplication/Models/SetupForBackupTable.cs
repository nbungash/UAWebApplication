using System;
using System.Collections.Generic;

namespace UAWebApplication.Models;

public partial class SetupForBackupTable
{
    public int Id { get; set; }

    public string? ComputerName { get; set; }

    public string? ServerName { get; set; }

    public string? UserId { get; set; }

    public string? Password { get; set; }

    public string? BackupFolder { get; set; }
}
