using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.ContentViewModels
{
    public class CreateViewModel
    {
        [Key]
        //[Required]
        public Guid Guid { get; set; }

        //[Required]
        [ForeignKey("ApplicationUser")]
        public string UserId{ get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Display(Name = "Titulo")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string Title { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        public string Description { get; set; }

        [Display(Name = "Selecciona el archivo:")]
        [Required(ErrorMessage = "El campo no puede estar vacio.")]
        [NotMapped]
        // TODO Cambiar por el formato valido
        //[FileExtensions(Extensions = "txt", ErrorMessage ="Este formato no es valido.")]
        public IFormFile File { get; set; }
        public string AudioDescription { get; set; }
        
    }
}
