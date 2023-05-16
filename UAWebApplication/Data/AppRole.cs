using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAWebApplication.Data
{
    public class AppRole : IdentityRole
    {
        public AppRole() : base() { }
        
        public AppRole(string name, int? ResourceId) : base(name)
        {
            this.ResourceId = ResourceId;
        }
        public virtual int? ResourceId { get; set; }
    }
}
