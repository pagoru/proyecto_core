using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Models.ContentViewModels
{
    public class DetailsViewModel
    {
        public ApplicationContent ApplicationContent { get; set; }

        public bool IsAdmin { get; set; }
    }
}
