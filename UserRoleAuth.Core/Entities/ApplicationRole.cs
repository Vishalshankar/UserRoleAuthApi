using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRoleAuth.Core.Entities
{
    public class ApplicationRole: IdentityRole
    {
        public string? Description { get; set; }
    }
}
