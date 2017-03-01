using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace proyecto_core.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }

        public string getUserNameToDisplay()
        {
            var at_username = $"@{UserName}";
            if (Name == null)
                return at_username;

            if(Name.Length == 0)
                return at_username;

            return Name;
        }
    }
}
