using System.ComponentModel;    
using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class CategoryModel
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [DisplayName("Name")]
        public string? CategoryName { get; set; }
        [DisplayName("Discription")]
        public string? Discription { get; set; }
        public DateTime CreatedDate { get; set; }=DateTime.Now;

    }
}
