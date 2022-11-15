using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuilkyBook.Model
{
    public class CoverType
    {
        [Key]
        public int id { get; set; }
        [Required]
        [DisplayName("Cover Type")]
        public string name { get; set; }
    }
}
