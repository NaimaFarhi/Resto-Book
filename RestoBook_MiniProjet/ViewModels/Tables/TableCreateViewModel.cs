using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.ViewModels.Tables
{
    public class TableCreateViewModel
    {
        [Required]
        [Range(1, 100)]
        public int TableNumber { get; set; }

        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
