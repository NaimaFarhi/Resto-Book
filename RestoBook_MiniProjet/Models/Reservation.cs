using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public string? GuestName { get; set; }

        [Required]
        public int RestaurantTableId { get; set; }
        public RestaurantTable RestaurantTable { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        public string Status { get; set; } = ReservationStatus.Pending;
        public string? SpecialRequest { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
