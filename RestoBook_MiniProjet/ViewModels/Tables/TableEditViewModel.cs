using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.ViewModels.Tables
{
    public class TableEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 100)]
        public int TableNumber { get; set; }

        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }

        public bool IsActive { get; set; }
    }
}
