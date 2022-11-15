using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BuilkyBook.Model
{
    public class Category
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [DisplayName("Display Order")]
        public int displayOrder { get; set; }

        public DateTime createdDate { get; set; } = DateTime.Now;
    }
}
