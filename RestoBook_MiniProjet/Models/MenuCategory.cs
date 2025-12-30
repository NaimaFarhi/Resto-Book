using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.Models
{
    public class MenuCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100)]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}