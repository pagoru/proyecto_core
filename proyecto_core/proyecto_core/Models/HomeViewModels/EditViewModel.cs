using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.HomeViewModels
{
    public class EditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Display(Name = "Titulo")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string Title { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string Description { get; set; }

        [Display(Name = "Audiodescripción")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string Audiodescription { get; set; }
    }
}
