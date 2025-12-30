using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.ViewModels.Menu
{
    public class MenuItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [MaxLength(200)]
        [Display(Name = "Item Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int MenuCategoryId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        // For display
        public string? CategoryName { get; set; }
    }
}