using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.ManageViewModels
{
    public class ChangeNameViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }

        [Display(Name = "Nombre")]
        public string NewName { get; set; }
    }
}
