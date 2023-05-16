using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAWebApplication.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? UserTheme { get; set; } 
        public string? DisplayName { get; set; }
    }
}
