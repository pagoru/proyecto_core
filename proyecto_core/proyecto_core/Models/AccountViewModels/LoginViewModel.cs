using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Nombre de usuario")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string UserName { get; set; }

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string Password { get; set; }

        [Display(Name = "¿Recordarme?")]
        public bool RememberMe { get; set; }
    }
}
