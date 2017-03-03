using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.ContentViewModels
{
    public class CreateViewModel : ApplicationContent
    {
        [Display(Name = "Titulo")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public override string Title { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public override string Description { get; set; }

        [NotMapped]
        [Display(Name = "Selecciona el archivo:")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        // TODO Cambiar por el formato valido
        //[FileExtensions(Extensions = "txt", ErrorMessage ="Este formato no es valido.")]
        public IFormFile File { get; set; }
        
    }
}
