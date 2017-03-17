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
        public Guid Id { get; set; }

        public virtual string Title { get; set; }
        public virtual string Description { get; set; }

        public string AudioDescription { get; set; }

        public string AddedDateTime { get; set; }

        public int Views { get; set; }
        public int DemoDownloads { get; set; }
        public int Downloads { get; set; }


        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }
        public string UserId { get; set; }

    }
}
