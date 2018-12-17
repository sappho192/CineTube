using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Cinetube.Models
{
    public class BoardModel
    {
        [Key]
        public int articleNo { get; set; }

        [Required]
        public string ID { get; set; }

        [Required]
        public string title { get; set; }

        [Required]
        public DateTime writeTime {get; set;}
    }
}
