using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.AdminViewModels
{
    public class AddUserRoleViewModel : DetailsRoleViewModel
    {
        public string RoleName { get; set; }
        public string UserName { get; set; }
    }
}
