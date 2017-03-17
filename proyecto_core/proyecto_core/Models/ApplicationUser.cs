using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using proyecto_core.Models.ContentViewModels;
using System.Collections.Generic;
using System.Linq;

namespace proyecto_core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }

        public virtual ICollection<ApplicationContent> ApplicationContentCollection { get; set; }



        public string getUserNameToDisplay()
        {
            var at_username = $"@{UserName}";
            if (Name == null)
                return at_username;

            if(Name.Length == 0)
                return at_username;

            return Name;
        }

        public bool IsInRole(IdentityRole role)
        {
            return Roles.FirstOrDefault(r => r.RoleId == role.Id) != null;
        }

    }
}
