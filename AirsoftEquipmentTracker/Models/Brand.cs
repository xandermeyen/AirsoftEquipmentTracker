using System.ComponentModel.DataAnnotations;

namespace AirsoftEquipmentTracker.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
    }
}
