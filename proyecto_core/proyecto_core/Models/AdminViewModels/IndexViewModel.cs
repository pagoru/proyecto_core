using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.AdminViewModels
{
    public class IndexViewModel
    {
        public List<IdentityRole> IdentityRoleList { get; set; }
    }
}
