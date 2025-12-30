using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }

        [Required]
        public int TableNumber { get; set; }

        [Required]
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Reservation> Reservations { get; set; }

    }
}
