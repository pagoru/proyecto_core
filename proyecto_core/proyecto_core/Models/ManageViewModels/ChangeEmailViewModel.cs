using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.ManageViewModels
{
    public class ChangeEmailViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }

        [ReadOnly(true)]
        [Display(Name = "Correo electrónico actual")]
        public string OldEmail { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Nuevo correo electrónico")]
        public string NewEmail { get; set; }

        [Display(Name = "Confirmar nuevo correo electrónico")]
        [Compare("NewEmail", ErrorMessage = "El nuevo correo electrónico de confirmación no coincide.")]
        public string ConfirmEmail { get; set; }
    }
}
