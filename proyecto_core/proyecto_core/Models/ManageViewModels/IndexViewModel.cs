using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace proyecto_core.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }


        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }

        public bool IsAdmin { get; set; }
    }
}
