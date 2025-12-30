using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.ViewModels.Reservations
{
    public class ReservationEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [Required]
        public int RestaurantTableId { get; set; }

        [Required]
        public string Status { get; set; }

        [MaxLength(500)]
        [Display(Name = "Special Request")]
        public string? SpecialRequest { get; set; }

        public string? GuestName { get; set; }
    }
}
