using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.ViewModels.Reservations
{
    public class ReservationCreateViewModel
    {
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ReservationDate { get; set; }

        public string? GuestName { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        [MaxLength(500)]
        [Display(Name = "Special Request")]
        public string? SpecialRequest { get; set; }
    }
}