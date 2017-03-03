using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.ContentViewModels
{
    public class ApplicationContent
    {
        [Key]
        public Guid Guid { get; set; }
        
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public string AudioDescription { get; set; }
    }
}
