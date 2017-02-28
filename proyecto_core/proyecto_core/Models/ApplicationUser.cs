using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace proyecto_core.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }

        public string getUsernameTodisplay()
        {
            if (Name == null)
                return "@" + UserName;

            if(Name.Length == 0)
                return "@" + UserName;

            return Name;
        }
    }
}
