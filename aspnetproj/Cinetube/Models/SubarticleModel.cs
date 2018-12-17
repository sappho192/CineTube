using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cinetube.Models
{
    public class SubarticleModel
    {
        [Key]
        public int subNo { get; set; }

        [Required]
        public string subID { get; set; }

        [Required]
        public string subContext { get; set; }
    }
}
