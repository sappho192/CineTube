using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cinetube.Models
{
    public class LoginModel
    {
        [Required]
        public string ID { get; set; }

        [Required]
        public string PW { get; set; }

    }
}
