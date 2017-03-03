using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Display(Name = "Nombre")]
        [StringLength(64, ErrorMessage = "El nombre debe tener {1} carácteres máximo.")]
        public string Name { get; set; }
        
        [Display(Name = "Nombre de usuario")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "El nombre de usuario debe tener al menos {2} carácteres y {1} de máximo.")]
        public string UserName { get; set; }
        
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string Email { get; set; }

        [Display(Name = "Confirmar correo electrónico")]
        [Compare("Email", ErrorMessage = "El correo electrónico de confirmación no coincide.")]
        public string ConfirmEmail { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La {0} debe tener al menos {2} carácteres y {1} de máximo.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña de confirmación no coincide.")]
        public string ConfirmPassword { get; set; }
    }
}
