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

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [NotMapped]
        public IFormFile File { get; set; }
        public string AudioDescription { get; set; }
        
    }
}
